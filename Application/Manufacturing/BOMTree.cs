using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

namespace Application.Manufacturing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BOMTree
{
    public const int EXPLOSION = 0;
    public const int EXPLOSION_SINGLE_LEVEL = 1;
    public const int EXPLOSION_MANUFACTURING = 2;
    public const int IMPLOSION = 3;
    public BOMNode Root { get; set; }
    public BOMNode OriginalNode { get; set; }
    public decimal RootQuantity { get; set; }
    public decimal RootAmount { get; set; }
    public DateTime InDate { get; set; }
    public string BomTypeId { get; set; }
    public Product InputProduct { get; set; }

    public BOMTree(string productIs, string bomTypeId, DateTime inDate)
    {
        BomTypeId = bomTypeId;
        InDate = inDate;
        RootQuantity = 1;
        RootAmount = 0;
    }

    public bool IsConfigured()
    {
        var notConfiguredParts = new List<BOMNode>();
        Root.IsConfigured(notConfiguredParts);
        return notConfiguredParts.Count == 0;
    }

    // Getter for RootQuantity
    public decimal GetRootQuantity()
    {
        return RootQuantity;
    }

    // Getter for InDate
    public DateTime GetInDate()
    {
        return InDate;
    }

    // Getter for BomTypeId
    public string GetBomTypeId()
    {
        return BomTypeId;
    }


    // Setter for RootQuantity
    public void SetRootQuantity(decimal rootQuantity)
    {
        RootQuantity = rootQuantity;
    }

    // Getter for RootAmount
    public decimal GetRootAmount()
    {
        return RootAmount;
    }

    // Setter for RootAmount
    public void SetRootAmount(decimal rootAmount)
    {
        RootAmount = rootAmount;
    }

    // Getter for Root (BOMNode)
    public BOMNode GetRoot()
    {
        return Root;
    }

    public void Print(List<BOMNode> arr, int initialDepth = 0, bool excludeWIPs = false)
    {
        if (Root != null)
        {
            //Root.Print(arr, RootQuantity, initialDepth, excludeWIPs);
        }
    }

    public void SumQuantities(Dictionary<string, BOMNode> quantityPerNode)
    {
        if (Root != null)
        {
            Root.SumQuantity(quantityPerNode);
        }
    }

    public List<string> GetAllProductsId()
    {
        var nodeArr = new List<BOMNode>();
        var productsId = new List<string>();
        Print(nodeArr);
        foreach (var node in nodeArr)
        {
            productsId.Add(node.Product.ProductId);
        }

        return productsId;
    }

    public void GetProductsInPackages(List<BOMNode> arr)
    {
        if (Root != null)
        {
            Root.GetProductsInPackages(arr, RootQuantity, 0, false);
        }
    }
}