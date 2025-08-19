using System.Text;
using Domain;

namespace Application.Manufacturing;

public class BOMNode
{
    private readonly Domain.Product _product;
    private string bomTypeId;
    private List<ProductAssoc> children;
    private List<BOMNode> childrenNodes;
    private int depth;
    private BOMNode parentNode;
    private ProductAssoc productAssoc;
    private string productForRules;
    private decimal quantity;


    private ProductManufacturingRule ruleApplied;
    private BOMNode substitutedNode;
    private BOMTree tree;

    public decimal QuantityMultiplier { get; private set; }
    public decimal ScrapFactor { get; private set; }

    public void SetQuantityMultiplier(decimal quantityMultiplier)
    {
        if (quantityMultiplier != 0)
        {
            QuantityMultiplier = quantityMultiplier;
        }
    }

    public void SetScrapFactor(decimal scrapFactor)
    {
        ScrapFactor = scrapFactor;
    }


    public BOMNode(Domain.Product product)
    {
        _product = product;
        Product = _product;
    }

    public BOMTree Tree { get; private set; }
    public BOMNode ParentNode { get; private set; }
    public BOMNode SubstitutedNode { get; private set; }
    public Domain.Product Product { get; }
    public ProductAssoc ProductAssoc { get; private set; }
    public List<ProductAssoc> Children { get; set; }
    public List<BOMNode> ChildrenNodes { get; set; }
    public int Depth { get; private set; }
    public decimal Quantity { get; set; }
    public string BomTypeId { get; set; }


    public BOMNode GetParentNode()
    {
        return ParentNode;
    }

    public BOMNode GetRootNode()
    {
        return ParentNode != null ? ParentNode.GetRootNode() : this;
    }

    public void SetParentNode(BOMNode parentNode)
    {
        ParentNode = parentNode;
    }

    public void Print(StringBuilder sb, decimal quantity, int depth)
    {
        // --- Added Section: Set Node Quantity ---
        // Update the current node's Quantity based on the parent quantity.
        this.Quantity = quantity;

        for (var i = 0; i < depth; i++) sb.Append("<b>&nbsp;*&nbsp;</b>");

        sb.Append(Product.ProductId);
        sb.Append(" - ");
        sb.Append(quantity);

        ProductAssoc oneChild = null;
        BOMNode oneChildNode = null;
        depth++;

        for (var i = 0; i < Children.Count; i++)
        {
            oneChild = Children[i];
            decimal bomQuantity = 0;

            try
            {
                bomQuantity = oneChild.Quantity ?? 1;
            }
            catch (Exception)
            {
                bomQuantity = 1;
            }

            oneChildNode = ChildrenNodes[i];
            sb.Append("<br/>");

            if (oneChildNode != null)
            {
                // --- Updated: Pass calculated child quantity ---
                oneChildNode.Print(sb, quantity * bomQuantity, depth);
            }
        }
    }

    public void GetProductsInPackages(List<BOMNode> arr, decimal quantity, int depth, bool excludeWIPs)
    {
        this.depth = depth;
        this.quantity = quantity * QuantityMultiplier * ScrapFactor;

        // Visit the current node first
        if (!string.IsNullOrEmpty(Product?.DefaultShipmentBoxTypeId))
        {
            arr.Add(this);
        }
        else
        {
            depth++; // Increase depth for children nodes
            for (var i = 0; i < ChildrenNodes.Count; i++)
            {
                var oneChildNode = ChildrenNodes[i];
                if (excludeWIPs && oneChildNode?.Product?.ProductTypeId == "WIP") continue;

                if (oneChildNode != null) oneChildNode.GetProductsInPackages(arr, this.quantity, depth, excludeWIPs);
            }
        }
    }


    public bool IsVirtual()
    {
        return _product.IsVirtual != null && _product.IsVirtual == "Y";
    }

    public void IsConfigured(List<BOMNode> arr)
    {
        // First of all, we visit the current node.
        if (IsVirtual()) arr.Add(this);

        // Now (recursively) we visit the children.
        BOMNode oneChildNode = null;
        for (var i = 0; i < children.Count; i++)
        {
            oneChildNode = childrenNodes[i];
            if (oneChildNode != null) oneChildNode.IsConfigured(arr);
        }
    }

    public decimal getQuantity()
    {
        return quantity;
    }

    public void setQuantity(decimal quantity)
    {
        this.quantity = quantity;
    }

    public int getDepth()
    {
        return depth;
    }

    public Domain.Product getProduct()
    {
        return _product;
    }

    public BOMNode getSubstitutedNode()
    {
        return substitutedNode;
    }

    public void SetSubstitutedNode(BOMNode substitutedNode)
    {
        this.substitutedNode = substitutedNode;
    }

    public string GetRootProductForRules()
    {
        return getParentNode().GetProductForRules();
    }


    public string GetProductForRules()
    {
        return productForRules;
    }


    public void SetProductForRules(string productForRules)
    {
        this.productForRules = productForRules;
    }


    public string getBomTypeId()
    {
        return bomTypeId;
    }


    public BOMNode getParentNode()
    {
        return parentNode;
    }


    public ProductManufacturingRule getRuleApplied()
    {
        return ruleApplied;
    }


    public void setRuleApplied(ProductManufacturingRule ruleApplied)
    {
        this.ruleApplied = ruleApplied;
    }


    public List<BOMNode> getChildrenNodes()
    {
        return childrenNodes;
    }

    public void setChildrenNodes(List<BOMNode> childrenNodes)
    {
        this.childrenNodes = childrenNodes;
    }


    public ProductAssoc getProductAssoc()
    {
        return productAssoc;
    }


    public void setProductAssoc(ProductAssoc productAssoc)
    {
        this.productAssoc = productAssoc;
    }

    public void SetTree(BOMTree tree)
    {
        this.tree = tree;
    }

    public BOMTree getTree()
    {
        return tree;
    }

    public async Task SumQuantity(Dictionary<string, BOMNode> nodes, int depth = 0)
    {
        // Attempt to retrieve the existing node from the dictionary
        if (!nodes.TryGetValue(Product.ProductId, out BOMNode sameNode))
        {
            // Create a new BOMNode for the current product (directly instead of using the factory)
            sameNode = new BOMNode(Product);
            nodes[Product.ProductId] = sameNode;
        }

        // Add the current node's quantity to the matching node in the dictionary
        sameNode.Quantity += Quantity;

        // Recursively sum the quantities of the child nodes
        for (int i = 0; i < ChildrenNodes.Count; i++)
        {
            var oneChildNode = ChildrenNodes[i];
            if (oneChildNode != null)
            {
                await oneChildNode.SumQuantity(nodes, depth + 1); // Recursively call SumQuantity with incremented depth
            }
        }
    }
}