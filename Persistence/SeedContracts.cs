using Bogus;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Persistence;

public class SeedContracts
{
    public static async Task SeedData(DataContext context,
        UserManager<AppUserLogin> userManager, RoleManager<ApplicationRole> roleManager)
    {
        var dateNow = DateTime.UtcNow;
        var nowDateTime = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, dateNow.Minute,
            dateNow.Second, 0, DateTimeKind.Utc);


        //Mime Types
        if (!context.MimeTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/mime_types.json");
            var jsonData = File.ReadAllText(path);

            var mimeTypes = JsonConvert.DeserializeObject<List<MimeType>>(jsonData);
            await context.MimeTypes.AddRangeAsync(mimeTypes);
            await context.SaveChangesAsync();
        }

        //Data resource Types
        if (!context.DataResourceTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/data_resource_types.json");
            var jsonData = File.ReadAllText(path);

            var dataResourceTypes = JsonConvert.DeserializeObject<List<DataResourceType>>(jsonData);
            await context.DataResourceTypes.AddRangeAsync(dataResourceTypes);
            await context.SaveChangesAsync();
        }

        //Content Types
        if (!context.ContentTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/content_types.json");
            var jsonData = File.ReadAllText(path);

            var contentTypes = JsonConvert.DeserializeObject<List<ContentType>>(jsonData);
            await context.ContentTypes.AddRangeAsync(contentTypes);
            await context.SaveChangesAsync();
        }

        //Uom Types
        if (!context.UomTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/uom_types.json");
            var jsonData = File.ReadAllText(path);

            var uomTypes = JsonConvert.DeserializeObject<List<UomType>>(jsonData);
            await context.UomTypes.AddRangeAsync(uomTypes);
            await context.SaveChangesAsync();
        }

        //Shipment Types
        if (!context.ShipmentTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/shipment_types.json");
            var jsonData = File.ReadAllText(path);

            var shipmentTypes = JsonConvert.DeserializeObject<List<ShipmentType>>(jsonData);
            await context.ShipmentTypes.AddRangeAsync(shipmentTypes);
            await context.SaveChangesAsync();
        }

        //Product Category Types
        if (!context.ProductCategoryTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_category_types_medications.json");
            var jsonData = File.ReadAllText(path);

            var productCategoryTypes = JsonConvert.DeserializeObject<List<ProductCategoryType>>(jsonData);
            await context.ProductCategoryTypes.AddRangeAsync(productCategoryTypes);
            await context.SaveChangesAsync();
        }

        //Uom 
        if (!context.Uoms.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/UOMs.json");
            var jsonData = File.ReadAllText(path);

            var uoms = JsonConvert.DeserializeObject<List<Uom>>(jsonData);
            await context.Uoms.AddRangeAsync(uoms);
            await context.SaveChangesAsync();
        }

        //Uom conversion
        if (!context.UomConversions.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/uom_conversion.json");
            var jsonData = File.ReadAllText(path);

            var uomConversion = JsonConvert.DeserializeObject<List<UomConversion>>(jsonData);
            await context.UomConversions.AddRangeAsync(uomConversion);
            await context.SaveChangesAsync();
        }

        //Uom conversion dated
        if (!context.UomConversionDateds.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/uom_conversion_dated.json");
            var jsonData = File.ReadAllText(path);

            var uomConversionDated = JsonConvert.DeserializeObject<List<UomConversionDated>>(jsonData);
            await context.UomConversionDateds.AddRangeAsync(uomConversionDated);
            await context.SaveChangesAsync();
        }

        //Status Type 
        if (!context.StatusTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/status_types.json");
            var jsonData = File.ReadAllText(path);

            var statusTypes = JsonConvert.DeserializeObject<List<StatusType>>(jsonData);
            await context.StatusTypes.AddRangeAsync(statusTypes);
            await context.SaveChangesAsync();
        }

        //Status Item 
        if (!context.StatusItems.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/status_items.json");
            var jsonData = File.ReadAllText(path);

            var statusItems = JsonConvert.DeserializeObject<List<StatusItem>>(jsonData);
            await context.StatusItems.AddRangeAsync(statusItems);
            await context.SaveChangesAsync();
        }

        //Status Valid Changes
        if (!context.StatusValidChanges.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/status_valid_changes.json");
            var jsonData = File.ReadAllText(path);

            var statusValidChanges = JsonConvert.DeserializeObject<List<StatusValidChange>>(jsonData);
            await context.StatusValidChanges.AddRangeAsync(statusValidChanges);
            await context.SaveChangesAsync();
        }

        // Enumeration type
        if (!context.EnumerationTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/enumeration_types.json");
            var jsonData = File.ReadAllText(path);

            var enumerationTypes = JsonConvert.DeserializeObject<List<EnumerationType>>(jsonData);
            await context.EnumerationTypes.AddRangeAsync(enumerationTypes);
            await context.SaveChangesAsync();
        }

        // Enumeration
        if (!context.Enumerations.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/enumerations.json");
            var jsonData = File.ReadAllText(path);

            var enumerations = JsonConvert.DeserializeObject<List<Enumeration>>(jsonData);
            await context.Enumerations.AddRangeAsync(enumerations);
            await context.SaveChangesAsync();
        }

        // financial_account_types
        if (!context.FinAccountTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/fin_account_types.json");
            var jsonData = File.ReadAllText(path);

            var finAccountTypes = JsonConvert.DeserializeObject<List<FinAccountType>>(jsonData);
            await context.FinAccountTypes.AddRangeAsync(finAccountTypes);
            await context.SaveChangesAsync();
        }

        // financial_account_trans_types
        if (!context.FinAccountTransTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/fin_account_trans_types.json");
            var jsonData = File.ReadAllText(path);

            var finAccountTransTypes = JsonConvert.DeserializeObject<List<FinAccountTransType>>(jsonData);
            await context.FinAccountTransTypes.AddRangeAsync(finAccountTransTypes);
            await context.SaveChangesAsync();
        }


        //Product Category
        if (!context.ProductCategories.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_categories_contracts.json");
            var jsonData = File.ReadAllText(path);

            var productCategories = JsonConvert.DeserializeObject<List<ProductCategory>>(jsonData);
            await context.ProductCategories.AddRangeAsync(productCategories);
            await context.SaveChangesAsync();
        }

        //Product Feature Appl Type
        if (!context.ProductFeatureApplTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_feature_appl_types.json");
            var jsonData = File.ReadAllText(path);

            var productFeatureApplTypes = JsonConvert.DeserializeObject<List<ProductFeatureApplType>>(jsonData);
            await context.ProductFeatureApplTypes.AddRangeAsync(productFeatureApplTypes);
            await context.SaveChangesAsync();
        }

        //Product Feature Type
        if (!context.ProductFeatureTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_feature_types.json");
            var jsonData = File.ReadAllText(path);

            var productFeatureTypes = JsonConvert.DeserializeObject<List<ProductFeatureType>>(jsonData);
            await context.ProductFeatureTypes.AddRangeAsync(productFeatureTypes);
            await context.SaveChangesAsync();
        }

        //Product Features
        if (!context.ProductFeatures.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_features.json");
            var jsonData = File.ReadAllText(path);

            var productFeatures = JsonConvert.DeserializeObject<List<ProductFeature>>(jsonData);
            await context.ProductFeatures.AddRangeAsync(productFeatures);
            await context.SaveChangesAsync();
        }


        //Role Type 
        if (!context.RoleTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/role_types.json");
            var jsonData = File.ReadAllText(path);

            var roleTypes = JsonConvert.DeserializeObject<List<RoleType>>(jsonData);
            await context.RoleTypes.AddRangeAsync(roleTypes);
            await context.SaveChangesAsync();
        }

        //Return Header Type 
        if (!context.ReturnHeaderTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/return_header_types.json");
            var jsonData = File.ReadAllText(path);

            var returnHeaderTypes = JsonConvert.DeserializeObject<List<ReturnHeaderType>>(jsonData);
            await context.ReturnHeaderTypes.AddRangeAsync(returnHeaderTypes);
            await context.SaveChangesAsync();
        }

        //Return Type 
        if (!context.ReturnTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/return_types.json");
            var jsonData = File.ReadAllText(path);

            var returnTypes = JsonConvert.DeserializeObject<List<ReturnType>>(jsonData);
            await context.ReturnTypes.AddRangeAsync(returnTypes);
            await context.SaveChangesAsync();
        }

        //SupplierPrefOrders
        if (!context.SupplierPrefOrders.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/supplier_pref_order_id.json");
            var jsonData = File.ReadAllText(path);

            var supplierPrefOrderIds = JsonConvert.DeserializeObject<List<SupplierPrefOrder>>(jsonData);
            await context.SupplierPrefOrders.AddRangeAsync(supplierPrefOrderIds);
            await context.SaveChangesAsync();
        }

        //Return Item Type 
        if (!context.ReturnItemTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/return_item_types.json");
            var jsonData = File.ReadAllText(path);

            var returnItemTypes = JsonConvert.DeserializeObject<List<ReturnItemType>>(jsonData);
            await context.ReturnItemTypes.AddRangeAsync(returnItemTypes);
            await context.SaveChangesAsync();
        }

        //Return Item Type Map
        if (!context.ReturnItemTypeMaps.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/return_item_type_maps.json");
            var jsonData = File.ReadAllText(path);

            var returnItemTypeMaps = JsonConvert.DeserializeObject<List<ReturnItemTypeMap>>(jsonData);
            await context.ReturnItemTypeMaps.AddRangeAsync(returnItemTypeMaps);
            await context.SaveChangesAsync();
        }

        //Return Reason
        if (!context.ReturnReasons.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/return_reasons.json");
            var jsonData = File.ReadAllText(path);

            var returnReasons = JsonConvert.DeserializeObject<List<ReturnReason>>(jsonData);
            await context.ReturnReasons.AddRangeAsync(returnReasons);
            await context.SaveChangesAsync();
        }

        // party types
        if (!context.PartyTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/party_types.json");
            var jsonData = File.ReadAllText(path);

            var partyTypes = JsonConvert.DeserializeObject<List<PartyType>>(jsonData);
            await context.PartyTypes.AddRangeAsync(partyTypes);
            await context.SaveChangesAsync();
        }


        // geo types
        if (!context.GeoTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/geo_types.json");
            var jsonData = File.ReadAllText(path);

            var geoTypes = JsonConvert.DeserializeObject<List<GeoType>>(jsonData);
            await context.GeoTypes.AddRangeAsync(geoTypes);
            await context.SaveChangesAsync();
        }

        // geos
        if (!context.Geos.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/geos.json");
            var jsonData = File.ReadAllText(path);

            var geos = JsonConvert.DeserializeObject<List<Geo>>(jsonData);
            await context.Geos.AddRangeAsync(geos);
            await context.SaveChangesAsync();
        }

        // contact mech types
        if (!context.ContactMechTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/contact_mech_types.json");
            var jsonData = File.ReadAllText(path);

            var contactMechTypes = JsonConvert.DeserializeObject<List<ContactMechType>>(jsonData);
            await context.ContactMechTypes.AddRangeAsync(contactMechTypes);
            await context.SaveChangesAsync();
        }


        // contact mech purpose types
        if (!context.ContactMechPurposeTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/contact_mech_purpose_types.json");
            var jsonData = File.ReadAllText(path);

            var contactMechPurposeTypes = JsonConvert.DeserializeObject<List<ContactMechPurposeType>>(jsonData);
            await context.ContactMechPurposeTypes.AddRangeAsync(contactMechPurposeTypes);
            await context.SaveChangesAsync();
        }


        // Product Price Type
        if (!context.ProductPriceTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_price_types.json");
            var jsonData = File.ReadAllText(path);

            var productPriceTypes = JsonConvert.DeserializeObject<List<ProductPriceType>>(jsonData);
            await context.ProductPriceTypes.AddRangeAsync(productPriceTypes);
            await context.SaveChangesAsync();
        }

        // Product Price Purpose
        if (!context.ProductPricePurposes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_price_purposes.json");
            var jsonData = File.ReadAllText(path);

            var productPricePurposes = JsonConvert.DeserializeObject<List<ProductPricePurpose>>(jsonData);
            await context.ProductPricePurposes.AddRangeAsync(productPricePurposes);
            await context.SaveChangesAsync();
        }


        // Product Types
        if (!context.ProductTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_types.json");
            var jsonData = File.ReadAllText(path);

            var productTypes = JsonConvert.DeserializeObject<List<ProductType>>(jsonData);
            await context.ProductTypes.AddRangeAsync(productTypes);
            await context.SaveChangesAsync();
        }

        // Product Assoc Types
        if (!context.ProductAssocTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_assoc_types.json");
            var jsonData = File.ReadAllText(path);

            var productAssocTypes = JsonConvert.DeserializeObject<List<ProductAssocType>>(jsonData);
            await context.ProductAssocTypes.AddRangeAsync(productAssocTypes);
            await context.SaveChangesAsync();
        }

        // facility types
        if (!context.FacilityTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/facility_types.json");
            var jsonData = File.ReadAllText(path);

            var facilityTypes = JsonConvert.DeserializeObject<List<FacilityType>>(jsonData);
            await context.FacilityTypes.AddRangeAsync(facilityTypes);
            await context.SaveChangesAsync();
        }


        // facilities
        if (!context.Facilities.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/facilities.json");
            var jsonData = File.ReadAllText(path);

            var facilities = JsonConvert.DeserializeObject<List<Facility>>(jsonData);
            await context.Facilities.AddRangeAsync(facilities);
            await context.SaveChangesAsync();
        }


        // facility locations
        if (!context.FacilityLocations.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/facility_locations.json");
            var jsonData = File.ReadAllText(path);

            var facilityLocations = JsonConvert.DeserializeObject<List<FacilityLocation>>(jsonData);
            await context.FacilityLocations.AddRangeAsync(facilityLocations);
            await context.SaveChangesAsync();
        }

        // Sequence Value Item
        if (!context.SequenceValueItems.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/sequence_value_items.json");
            var jsonData = File.ReadAllText(path);

            var sequenceValueItmes = JsonConvert.DeserializeObject<List<SequenceValueItem>>(jsonData);
            await context.SequenceValueItems.AddRangeAsync(sequenceValueItmes);
            await context.SaveChangesAsync();
        }

        if (!context.GoodIdentificationTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/good_identification_types.json");
            var jsonData = File.ReadAllText(path);

            var goodIdentificationTypes = JsonConvert.DeserializeObject<List<GoodIdentificationType>>(jsonData);
            await context.GoodIdentificationTypes.AddRangeAsync(goodIdentificationTypes);
            await context.SaveChangesAsync();
        }


        if (!context.Products.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/products_contracts.json");
            var jsonData = File.ReadAllText(path);

            var products = JsonConvert.DeserializeObject<List<Product>>(jsonData);
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        /*if (!context.ProductPrices.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_prices_medications.json");
            var jsonData = File.ReadAllText(path);

            var productPrices = JsonConvert.DeserializeObject<List<ProductPrice>>(jsonData);
            await context.ProductPrices.AddRangeAsync(productPrices);
            await context.SaveChangesAsync();
        }*/


        /*if (!context.ProductAssocs.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_assocs_clothes.json");
            var jsonData = File.ReadAllText(path);

            var productAssocs = JsonConvert.DeserializeObject<List<ProductAssoc>>(jsonData);
            await context.ProductAssocs.AddRangeAsync(productAssocs);
            await context.SaveChangesAsync();
        }*/


        // Seeding ProductCategoryMembers
        /*{if (!context.ProductCategoryMembers.Any())
        
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_category_members_medications.json");
            var jsonData = File.ReadAllText(path);

            var productCategoryMembers = JsonConvert.DeserializeObject<List<ProductCategoryMember>>(jsonData);
            await context.ProductCategoryMembers.AddRangeAsync(productCategoryMembers);
            await context.SaveChangesAsync();
        }*/


        // inventory item types
        if (!context.InventoryItemTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/inventory_item_types.json");
            var jsonData = File.ReadAllText(path);

            var inventoryItemTypes = JsonConvert.DeserializeObject<List<InventoryItemType>>(jsonData);
            await context.InventoryItemTypes.AddRangeAsync(inventoryItemTypes);
            await context.SaveChangesAsync();
        }

        // Seeding ProductFacilities
        /*if (!context.ProductFacilities.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_facilities_medications.json");
            var jsonData = File.ReadAllText(path);

            var productFacilities = JsonConvert.DeserializeObject<List<ProductFacility>>(jsonData);
            await context.ProductFacilities.AddRangeAsync(productFacilities);
            await context.SaveChangesAsync();
        }*/


        // cust request types
        if (!context.CustRequestTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/cust_request_types.json");
            var jsonData = File.ReadAllText(path);

            var custRequestTypes = JsonConvert.DeserializeObject<List<CustRequestType>>(jsonData);
            await context.CustRequestTypes.AddRangeAsync(custRequestTypes);
            await context.SaveChangesAsync();
        }

        // quote types
        if (!context.QuoteTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/quote_types.json");
            var jsonData = File.ReadAllText(path);

            var quoteTypes = JsonConvert.DeserializeObject<List<QuoteType>>(jsonData);
            await context.QuoteTypes.AddRangeAsync(quoteTypes);
            await context.SaveChangesAsync();
        }

        // order types
        if (!context.OrderTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/order_types.json");
            var jsonData = File.ReadAllText(path);

            var orderTypes = JsonConvert.DeserializeObject<List<OrderType>>(jsonData);
            await context.OrderTypes.AddRangeAsync(orderTypes);
            await context.SaveChangesAsync();
        }


        // order item types
        if (!context.OrderItemTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/order_item_types.json");
            var jsonData = File.ReadAllText(path);

            var orderItemTypes = JsonConvert.DeserializeObject<List<OrderItemType>>(jsonData);
            await context.OrderItemTypes.AddRangeAsync(orderItemTypes);
            await context.SaveChangesAsync();
        }


        // Order Adjustments type
        if (!context.OrderAdjustmentTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/order_adjustment_types.json");
            var jsonData = File.ReadAllText(path);

            var orderAdjustmentTypes = JsonConvert.DeserializeObject<List<OrderAdjustmentType>>(jsonData);
            await context.OrderAdjustmentTypes.AddRangeAsync(orderAdjustmentTypes);
            await context.SaveChangesAsync();
        }


        // gl_account_types
        if (!context.GlAccountTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/gl_account_types.json");
            var jsonData = File.ReadAllText(path);

            var glAccountTypes = JsonConvert.DeserializeObject<List<GlAccountType>>(jsonData);
            await context.GlAccountTypes.AddRangeAsync(glAccountTypes);
            await context.SaveChangesAsync();
        }

        // gl_resource_types
        if (!context.GlResourceTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/gl_resource_types.json");
            var jsonData = File.ReadAllText(path);

            var glResourceTypes = JsonConvert.DeserializeObject<List<GlResourceType>>(jsonData);
            await context.GlResourceTypes.AddRangeAsync(glResourceTypes);
            await context.SaveChangesAsync();
        }

        // gl_account_classes
        if (!context.GlAccountClasses.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/gl_account_classes.json");
            var jsonData = File.ReadAllText(path);

            var glAccountClasses = JsonConvert.DeserializeObject<List<GlAccountClass>>(jsonData);
            await context.GlAccountClasses.AddRangeAsync(glAccountClasses);
            await context.SaveChangesAsync();
        }

        // gl_xbrl_classes
        if (!context.GlXbrlClasses.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/gl_xbrl_classes.json");
            var jsonData = File.ReadAllText(path);

            var glXbrlClasses = JsonConvert.DeserializeObject<List<GlXbrlClass>>(jsonData);
            await context.GlXbrlClasses.AddRangeAsync(glXbrlClasses);
            await context.SaveChangesAsync();
        }

        // gl_accounts
        if (!context.GlAccounts.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/gl_accounts.json");
            var jsonData = File.ReadAllText(path);

            var glAccounts = JsonConvert.DeserializeObject<List<GlAccount>>(jsonData);
            await context.GlAccounts.AddRangeAsync(glAccounts);
            await context.SaveChangesAsync();
        }

        // Payment Method type
        if (!context.PaymentMethodTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/payment_method_types.json");
            var jsonData = File.ReadAllText(path);

            var paymentMethodTypes = JsonConvert.DeserializeObject<List<PaymentMethodType>>(jsonData);
            await context.PaymentMethodTypes.AddRangeAsync(paymentMethodTypes);
            await context.SaveChangesAsync();
        }


        // gl account category types
        if (!context.GlAccountCategoryTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/gl_account_category_types.json");
            var jsonData = File.ReadAllText(path);

            var glAccountCategoryTypes = JsonConvert.DeserializeObject<List<GlAccountCategoryType>>(jsonData);
            await context.GlAccountCategoryTypes.AddRangeAsync(glAccountCategoryTypes);
            await context.SaveChangesAsync();
        }

        // gl account categories
        if (!context.GlAccountCategories.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/gl_account_categories.json");
            var jsonData = File.ReadAllText(path);

            var glAccountCategories = JsonConvert.DeserializeObject<List<GlAccountCategory>>(jsonData);
            await context.GlAccountCategories.AddRangeAsync(glAccountCategories);
            await context.SaveChangesAsync();
        }


        //invoice types 
        if (!context.InvoiceTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/invoice_types.json");
            var jsonData = File.ReadAllText(path);

            var invoiceTypes = JsonConvert.DeserializeObject<List<InvoiceType>>(jsonData);
            await context.InvoiceTypes.AddRangeAsync(invoiceTypes);
            await context.SaveChangesAsync();
        }

        //invoice item types 
        if (!context.InvoiceItemTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/invoice_item_types.json");
            var jsonData = File.ReadAllText(path);

            var invoiceItemTypes = JsonConvert.DeserializeObject<List<InvoiceItemType>>(jsonData);
            await context.InvoiceItemTypes.AddRangeAsync(invoiceItemTypes);
            await context.SaveChangesAsync();
        }

        // payment type
        if (!context.PaymentTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/payment_types.json");
            var jsonData = File.ReadAllText(path);

            var paymentTypes = JsonConvert.DeserializeObject<List<PaymentType>>(jsonData);
            await context.PaymentTypes.AddRangeAsync(paymentTypes);
            await context.SaveChangesAsync();
        }

        // term type
        if (!context.TermTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/term_types.json");
            var jsonData = File.ReadAllText(path);

            var termTypes = JsonConvert.DeserializeObject<List<TermType>>(jsonData);
            await context.TermTypes.AddRangeAsync(termTypes);
            await context.SaveChangesAsync();
        }

        // agreement type
        if (!context.AgreementTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/agreement_types.json");
            var jsonData = File.ReadAllText(path);

            var agreementTypes = JsonConvert.DeserializeObject<List<AgreementType>>(jsonData);
            await context.AgreementTypes.AddRangeAsync(agreementTypes);
            await context.SaveChangesAsync();
        }

        // agreement Item type
        if (!context.AgreementItemTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/agreement_item_types.json");
            var jsonData = File.ReadAllText(path);

            var agreementItemTypes = JsonConvert.DeserializeObject<List<AgreementItemType>>(jsonData);
            await context.AgreementItemTypes.AddRangeAsync(agreementItemTypes);
            await context.SaveChangesAsync();
        }


        // rejection reasons
        if (!context.RejectionReasons.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/rejection_reasons.json");
            var jsonData = File.ReadAllText(path);

            var rejectionReasons = JsonConvert.DeserializeObject<List<RejectionReason>>(jsonData);
            await context.RejectionReasons.AddRangeAsync(rejectionReasons);
            await context.SaveChangesAsync();
        }

        // Seeding Parties
        if (!context.Parties.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/parties.json");
            var jsonData = File.ReadAllText(path);

            var parties = JsonConvert.DeserializeObject<List<Party>>(jsonData);
            await context.Parties.AddRangeAsync(parties);
            await context.SaveChangesAsync();

            var facilities = context.Facilities.ToList();
            foreach (var facility in facilities)
            {
                facility.OwnerPartyId = "Company";
            }

            await context.SaveChangesAsync();
        }


        // Seeding PartyGroups
        if (!context.PartyGroups.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/party_groups.json");
            var jsonData = File.ReadAllText(path);

            var partyGroups = JsonConvert.DeserializeObject<List<PartyGroup>>(jsonData);
            await context.PartyGroups.AddRangeAsync(partyGroups);
            await context.SaveChangesAsync();
        }

        // Seeding PartyRoles
        if (!context.PartyRoles.Any())
        {
            // Select all records from the Parties table
            var parties = context.Parties.ToList();

            foreach (var party in parties)
            {
                if (party.MainRole == "CUSTOMER")
                {
                    string[] customerRoles =
                    {
                        "BILL_TO_CUSTOMER", "CONTACT", "PLACING_CUSTOMER",
                        "SHIP_TO_CUSTOMER", "END_USER_CUSTOMER", "CUSTOMER"
                    };

                    foreach (var role in customerRoles)
                    {
                        context.PartyRoles.Add(new PartyRole
                        {
                            PartyId = party.PartyId,
                            RoleTypeId = role,
                            LastUpdatedStamp = DateTime.UtcNow,
                            LastUpdatedTxStamp = DateTime.UtcNow,
                            CreatedStamp = DateTime.UtcNow,
                            CreatedTxStamp = DateTime.UtcNow
                        });
                    }
                }
                else if (party.MainRole == "SUPPLIER")
                {
                    string[] supplierRoles =
                    {
                        "SUPPLIER_AGENT", "SHIP_FROM_VENDOR", "BILL_FROM_VENDOR",
                        "SUPPLIER", "ACCOUNT"
                    };

                    foreach (var role in supplierRoles)
                    {
                        context.PartyRoles.Add(new PartyRole
                        {
                            PartyId = party.PartyId,
                            RoleTypeId = role,
                            LastUpdatedStamp = DateTime.UtcNow,
                            LastUpdatedTxStamp = DateTime.UtcNow,
                            CreatedStamp = DateTime.UtcNow,
                            CreatedTxStamp = DateTime.UtcNow
                        });
                    }
                }
                else if (party.PartyId == "Company")
                {
                    string[] companyRoles =
                    {
                        "CARRIER", "INTERNAL_ORGANIZATIO", "BILL_TO_CUSTOMER",
                        "BILL_FROM_VENDOR", "ACCOUNT", "_NA_"
                    };

                    foreach (var role in companyRoles)
                    {
                        context.PartyRoles.Add(new PartyRole
                        {
                            PartyId = party.PartyId,
                            RoleTypeId = role,
                            LastUpdatedStamp = DateTime.UtcNow,
                            LastUpdatedTxStamp = DateTime.UtcNow,
                            CreatedStamp = DateTime.UtcNow,
                            CreatedTxStamp = DateTime.UtcNow
                        });
                    }
                }
                else if (party.PartyId == "_NA_")
                {
                    string[] naRoles =
                    {
                        "_NA_", "CARRIER"
                    };

                    foreach (var role in naRoles)
                    {
                        context.PartyRoles.Add(new PartyRole
                        {
                            PartyId = party.PartyId,
                            RoleTypeId = role,
                            LastUpdatedStamp = DateTime.UtcNow,
                            LastUpdatedTxStamp = DateTime.UtcNow,
                            CreatedStamp = DateTime.UtcNow,
                            CreatedTxStamp = DateTime.UtcNow
                        });
                    }
                }
                else if (party.MainRole == "TAX_AUTHORITY")
                {
                    context.PartyRoles.Add(new PartyRole
                    {
                        PartyId = party.PartyId,
                        RoleTypeId = "TAX_AUTHORITY",
                        LastUpdatedStamp = DateTime.UtcNow,
                        LastUpdatedTxStamp = DateTime.UtcNow,
                        CreatedStamp = DateTime.UtcNow,
                        CreatedTxStamp = DateTime.UtcNow
                    });
                }
                else if (new[] { "OPERATOR", "MACHINE_OPERATOR", "ASSEMBLY_OPERATOR", "PACKAGING_OPERATOR" }.Contains(
                             party.MainRole))
                {
                    string[] operatorRoles =
                    {
                        "OPERATOR", "MACHINE_OPERATOR", "ASSEMBLY_OPERATOR", "PACKAGING_OPERATOR"
                    };

                    foreach (var role in operatorRoles)
                    {
                        context.PartyRoles.Add(new PartyRole
                        {
                            PartyId = party.PartyId,
                            RoleTypeId = role,
                            LastUpdatedStamp = DateTime.UtcNow,
                            LastUpdatedTxStamp = DateTime.UtcNow,
                            CreatedStamp = DateTime.UtcNow,
                            CreatedTxStamp = DateTime.UtcNow
                        });
                    }
                }
                else if (party.MainRole == "EMPLOYEE")
                {
                    string[] employeeRoles =
                    {
                        "AGENT", "SALES_REP"
                    };

                    foreach (var role in employeeRoles)
                    {
                        context.PartyRoles.Add(new PartyRole
                        {
                            PartyId = party.PartyId,
                            RoleTypeId = role,
                            LastUpdatedStamp = DateTime.UtcNow,
                            LastUpdatedTxStamp = DateTime.UtcNow,
                            CreatedStamp = DateTime.UtcNow,
                            CreatedTxStamp = DateTime.UtcNow
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        if (!context.BillingAccounts.Any())
        {
            var parties = await context.Parties.ToListAsync();


            // Process each party with MainRole = "CUSTOMER"
            int billingAccountCounter = 300; // Initialize the counter starting at 300

            foreach (var party in parties.Where(p => p.MainRole == "CUSTOMER"))
            {
                // Generate BillingAccountId using the counter
                var billingAccountId = $"BA-{billingAccountCounter++:D6}";

                // Create a new BillingAccount
                var billingAccount = new BillingAccount
                {
                    BillingAccountId = billingAccountId, // Use the counter-based ID
                    AccountLimit = 10000.00m,
                    AccountCurrencyUomId = "EGP",
                    FromDate = DateTime.Now,
                    ThruDate = null,
                    Description = $"Billing Account for {party.Description}",
                    ExternalAccountId = null,
                    CreatedStamp = DateTime.Now,
                    LastUpdatedStamp = DateTime.Now
                };

                await context.BillingAccounts.AddAsync(billingAccount);

                // Create a BillingAccountRole record
                var billingAccountRole = new BillingAccountRole
                {
                    BillingAccountId = billingAccount.BillingAccountId,
                    PartyId = party.PartyId,
                    RoleTypeId = "BILL_TO_CUSTOMER",
                    FromDate = DateTime.Now,
                    ThruDate = null,
                    CreatedStamp = DateTime.Now,
                    LastUpdatedStamp = DateTime.Now
                };

                await context.BillingAccountRoles.AddAsync(billingAccountRole);

                // Create a BillingAccountTerm record
                var billingAccountTerm = new BillingAccountTerm
                {
                    BillingAccountTermId = Guid.NewGuid().ToString(),
                    BillingAccountId = billingAccount.BillingAccountId,
                    TermTypeId = "FIN_PAYMENT_TERM",
                    TermValue = null,
                    TermDays = 30,
                    UomId = null,
                    CreatedStamp = DateTime.Now,
                    LastUpdatedStamp = DateTime.Now
                };

                await context.BillingAccountTerms.AddAsync(billingAccountTerm);
            }

            await context.SaveChangesAsync();
        }


        // Check and seed Persons table
        if (!context.Persons.Any())
        {
            // Select all records from the Parties table
            var parties = context.Parties.ToList();

            foreach (var party in parties)
            {
                if (party.PartyTypeId == "PERSON")
                {
                    // Split the Description to get first name and last name
                    var nameParts = party.Description?.Split(' ');
                    string firstName = nameParts?.FirstOrDefault() ?? "DefaultFirstName";
                    string lastName = nameParts?.LastOrDefault() ?? "DefaultLastName";

                    // Create a new Person record
                    context.Persons.Add(new Domain.Person
                    {
                        PartyId = party.PartyId,
                        Salutation = null,
                        FirstName = firstName,
                        MiddleName = null,
                        LastName = lastName,
                        PersonalTitle = null,
                        Suffix = null,
                        Nickname = null,
                        FirstNameLocal = null,
                        MiddleNameLocal = null,
                        LastNameLocal = null,
                        OtherLocal = null,
                        MemberId = null,
                        Gender = null,
                        BirthDate = null,
                        DeceasedDate = null,
                        Height = null,
                        Weight = null,
                        MothersMaidenName = null,
                        MaritalStatus = null,
                        SocialSecurityNumber = null,
                        PassportNumber = null,
                        PassportExpireDate = null,
                        TotalYearsWorkExperience = null,
                        Comments = null,
                        EmploymentStatusEnumId = null,
                        ResidenceStatusEnumId = null,
                        Occupation = null,
                        YearsWithEmployer = null,
                        MonthsWithEmployer = null,
                        ExistingCustomer = null,
                        CardId = null,
                        LastUpdatedStamp = DateTime.UtcNow,
                        LastUpdatedTxStamp = DateTime.UtcNow,
                        CreatedStamp = DateTime.UtcNow,
                        CreatedTxStamp = DateTime.UtcNow
                    });
                }

                // Add PartyStatus record for each party
                context.PartyStatuses.Add(new PartyStatus
                {
                    StatusId = "PARTY_ENABLED",
                    PartyId = party.PartyId,
                    StatusDate = new DateTime(2001, 1, 1, 12, 0, 0),
                    ChangeByUserLoginId = null,
                    LastUpdatedStamp = DateTime.UtcNow,
                    LastUpdatedTxStamp = DateTime.UtcNow,
                    CreatedStamp = DateTime.UtcNow,
                    CreatedTxStamp = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
        }


        var contactMechIds = new Dictionary<(string PartyId, string ContactMechPurposeTypeId), string>();

        // Check if there are any records in the ContactMeches table
        if (!context.ContactMeches.Any())
        {
            // Loop through each party in the Parties table
            foreach (var party in context.Parties.ToList())
            {
                // Skip parties with MainRole 'EMPLOYEE' or '_NA_'
                if (party.MainRole == "EMPLOYEE" || party.MainRole == "CARRIER" || party.MainRole == "_NA_")
                {
                    continue;
                }

                // Generate a unique ID for each ContactMech type
                var contactMechIdTelcom = Guid.NewGuid().ToString();
                var contactMechIdEmail = Guid.NewGuid().ToString();
                var contactMechIdPostal = Guid.NewGuid().ToString();

                // Create a Telecom Number ContactMech
                var fakeContactMechTelcom = new ContactMech
                {
                    ContactMechId = contactMechIdTelcom,
                    ContactMechTypeId = "TELECOM_NUMBER",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                await context.ContactMeches.AddAsync(fakeContactMechTelcom);

                // Create an Email Address ContactMech
                var fakeContactMechEmail = new ContactMech
                {
                    ContactMechId = contactMechIdEmail,
                    ContactMechTypeId = "EMAIL_ADDRESS",
                    InfoString = new Faker().Person.Email,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                await context.ContactMeches.AddAsync(fakeContactMechEmail);

                // Create a Postal Address ContactMech
                var fakeContactMechPostal = new ContactMech
                {
                    ContactMechId = contactMechIdPostal,
                    ContactMechTypeId = "POSTAL_ADDRESS",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                await context.ContactMeches.AddAsync(fakeContactMechPostal);

                // Store ContactMechId values
                contactMechIds[(party.PartyId, "PRIMARY_PHONE")] = contactMechIdTelcom;
                contactMechIds[(party.PartyId, "PRIMARY_EMAIL")] = contactMechIdEmail;
                contactMechIds[(party.PartyId, "GENERAL_LOCATION")] = contactMechIdPostal;


                // Create a TelecomNumber record linked to the Telecom ContactMech
                var fakeTelcomNo = new TelecomNumber
                {
                    ContactMechId = contactMechIdTelcom,
                    ContactNumber = "011" + new Faker().Random.Replace("########"),
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                await context.TelecomNumbers.AddAsync(fakeTelcomNo);

                // Create a PostalAddress record linked to the Postal ContactMech
                var fakePostalAddress = new PostalAddress
                {
                    ContactMechId = contactMechIdPostal,
                    ToName = party.Description,
                    Address1 = new Faker().Address.StreetName(),
                    Address2 = new Faker().Address.BuildingNumber(),
                    CountryGeoId = "EGY",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                await context.PostalAddresses.AddAsync(fakePostalAddress);

                // Create PartyContactMech records linking the Party to each ContactMech
                var fakePartyContactMechTelcom = new PartyContactMech
                {
                    ContactMechId = contactMechIdTelcom,
                    PartyId = party.PartyId,
                    RoleTypeId = party.MainRole, // or any other logic for role assignment
                    CreatedStamp = nowDateTime,
                    FromDate = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                await context.PartyContactMeches.AddAsync(fakePartyContactMechTelcom);

                var fakePartyContactMechEmail = new PartyContactMech
                {
                    ContactMechId = contactMechIdEmail,
                    PartyId = party.PartyId,
                    RoleTypeId = party.MainRole, // or any other logic for role assignment
                    CreatedStamp = nowDateTime,
                    FromDate = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                await context.PartyContactMeches.AddAsync(fakePartyContactMechEmail);

                var fakePartyContactMechPostal = new PartyContactMech
                {
                    ContactMechId = contactMechIdPostal,
                    PartyId = party.PartyId,
                    RoleTypeId = party.MainRole, // or any other logic for role assignment
                    CreatedStamp = nowDateTime,
                    FromDate = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                await context.PartyContactMeches.AddAsync(fakePartyContactMechPostal);

                // Create PartyContactMechPurpose records defining the purpose of each ContactMech
                var fakePartyContactMechPurposeTelcom = new PartyContactMechPurpose
                {
                    ContactMechId = contactMechIdTelcom,
                    PartyId = party.PartyId,
                    ContactMechPurposeTypeId = "PRIMARY_PHONE",
                    CreatedStamp = nowDateTime,
                    FromDate = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                await context.PartyContactMechPurposes.AddAsync(fakePartyContactMechPurposeTelcom);

                var fakePartyContactMechPurposeEmail = new PartyContactMechPurpose
                {
                    ContactMechId = contactMechIdEmail,
                    PartyId = party.PartyId,
                    ContactMechPurposeTypeId = "PRIMARY_EMAIL",
                    CreatedStamp = nowDateTime,
                    FromDate = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                await context.PartyContactMechPurposes.AddAsync(fakePartyContactMechPurposeEmail);

                var fakePartyContactMechPurposePostal = new PartyContactMechPurpose
                {
                    ContactMechId = contactMechIdPostal,
                    PartyId = party.PartyId,
                    ContactMechPurposeTypeId = "GENERAL_LOCATION",
                    CreatedStamp = nowDateTime,
                    FromDate = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                await context.PartyContactMechPurposes.AddAsync(fakePartyContactMechPurposePostal);
            }

            // Save all changes to the database
            await context.SaveChangesAsync();
        }

        // Seeding BillingAccountRoles
        /*if (!context.BillingAccountRoles.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/billing_account_roles.json");
            var jsonData = File.ReadAllText(path);

            var billingAccountRoles = JsonConvert.DeserializeObject<List<BillingAccountRole>>(jsonData);
            await context.BillingAccountRoles.AddRangeAsync(billingAccountRoles);
            await context.SaveChangesAsync();
        }*/

        // Seeding PartyRelationshipTypes
        if (!context.PartyRelationshipTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/party_relationship_types.json");
            var jsonData = File.ReadAllText(path);

            var partyRelationshipTypes = JsonConvert.DeserializeObject<List<PartyRelationshipType>>(jsonData);
            await context.PartyRelationshipTypes.AddRangeAsync(partyRelationshipTypes);
            await context.SaveChangesAsync();
        }

        // Seeding PartyRelationships
        if (!context.PartyRelationships.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/party_relationships.json");
            var jsonData = File.ReadAllText(path);

            var partyRelationships = JsonConvert.DeserializeObject<List<PartyRelationship>>(jsonData);
            await context.PartyRelationships.AddRangeAsync(partyRelationships);
            await context.SaveChangesAsync();
        }


        // financial_accounts
        if (!context.FinAccounts.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/fin_accounts.json");
            var jsonData = File.ReadAllText(path);

            var finAccounts = JsonConvert.DeserializeObject<List<FinAccount>>(jsonData);
            await context.FinAccounts.AddRangeAsync(finAccounts);
            await context.SaveChangesAsync();
        }

        // financial_account_status
        if (!context.FinAccountStatuses.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/fin_account_status.json");
            var jsonData = File.ReadAllText(path);

            var finAccountStatuses = JsonConvert.DeserializeObject<List<FinAccountStatus>>(jsonData);
            await context.FinAccountStatuses.AddRangeAsync(finAccountStatuses);
            await context.SaveChangesAsync();
        }

        // fin accu gl accounts
        if (!context.FinAccountTypeGlAccounts.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/fin_acc_types_gl_account.json");
            var jsonData = File.ReadAllText(path);

            var finAccountTypesGlAccount = JsonConvert.DeserializeObject<List<FinAccountTypeGlAccount>>(jsonData);
            await context.FinAccountTypeGlAccounts.AddRangeAsync(finAccountTypesGlAccount);
            await context.SaveChangesAsync();
        }

        // fin account trans
        /*if (!context.FinAccountTrans.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/fin_account_trans.json");
            var jsonData = File.ReadAllText(path);

            var finAccountTrans = JsonConvert.DeserializeObject<List<FinAccountTran>>(jsonData);
            await context.FinAccountTrans.AddRangeAsync(finAccountTrans);
            await context.SaveChangesAsync();
        }*/


        // Payment Method type gl account
        if (!context.PaymentMethodTypeGlAccounts.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/payment_method_type_gl_accounts.json");
            var jsonData = File.ReadAllText(path);

            var paymentMethodTypeGlAccounts =
                JsonConvert.DeserializeObject<List<PaymentMethodTypeGlAccount>>(jsonData);
            await context.PaymentMethodTypeGlAccounts.AddRangeAsync(paymentMethodTypeGlAccounts);
            await context.SaveChangesAsync();
        }

        // gl journals
        if (!context.GlJournals.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/gl_journal.json");
            var jsonData = File.ReadAllText(path);

            var glJournals = JsonConvert.DeserializeObject<List<GlJournal>>(jsonData);
            await context.GlJournals.AddRangeAsync(glJournals);
            await context.SaveChangesAsync();
        }

        // Period Types
        if (!context.PeriodTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/period_types.json");
            var jsonData = File.ReadAllText(path);

            var periodTypes = JsonConvert.DeserializeObject<List<PeriodType>>(jsonData);
            await context.PeriodTypes.AddRangeAsync(periodTypes);
            await context.SaveChangesAsync();
        }

        // Custom Time Period 
        if (!context.CustomTimePeriods.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/custom_time_periods.json");
            var jsonData = File.ReadAllText(path);

            var customTimePeriods = JsonConvert.DeserializeObject<List<CustomTimePeriod>>(jsonData);
            await context.CustomTimePeriods.AddRangeAsync(customTimePeriods);
            await context.SaveChangesAsync();
        }

        // Custom Method Types
        if (!context.CustomMethodTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/custom_method_types.json");
            var jsonData = File.ReadAllText(path);

            var customMethodTypes = JsonConvert.DeserializeObject<List<CustomMethodType>>(jsonData);
            await context.CustomMethodTypes.AddRangeAsync(customMethodTypes);
            await context.SaveChangesAsync();
        }

        // Custom Methods 
        if (!context.CustomMethods.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/custom_methods.json");
            var jsonData = File.ReadAllText(path);

            var customMethods = JsonConvert.DeserializeObject<List<CustomMethod>>(jsonData);
            await context.CustomMethods.AddRangeAsync(customMethods);
            await context.SaveChangesAsync();
        }

        //Party Accounting Preference 
        if (!context.PartyAcctgPreferences.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/party_accounting_preferences.json");
            var jsonData = File.ReadAllText(path);

            var partyAccountingPreferences = JsonConvert.DeserializeObject<List<PartyAcctgPreference>>(jsonData);
            await context.PartyAcctgPreferences.AddRangeAsync(partyAccountingPreferences);
            await context.SaveChangesAsync();
        }

        // gl account organization
        if (!context.GlAccountOrganizations.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/gl_account_organization.json");
            var jsonData = File.ReadAllText(path);

            var glAccountOrganization = JsonConvert.DeserializeObject<List<GlAccountOrganization>>(jsonData);
            await context.GlAccountOrganizations.AddRangeAsync(glAccountOrganization);
            await context.SaveChangesAsync();
        }

        // accounting transaction entry types
        if (!context.AcctgTransEntryTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(),
                "Json/accounting_transaction_entry_types.json");
            var jsonData = File.ReadAllText(path);

            var acctgTransEntryTypes = JsonConvert.DeserializeObject<List<AcctgTransEntryType>>(jsonData);
            await context.AcctgTransEntryTypes.AddRangeAsync(acctgTransEntryTypes);
            await context.SaveChangesAsync();
        }

        // accounting transaction types
        if (!context.AcctgTransTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/accounting_transaction_types.json");
            var jsonData = File.ReadAllText(path);

            var acctgTransTypes = JsonConvert.DeserializeObject<List<AcctgTransType>>(jsonData);
            await context.AcctgTransTypes.AddRangeAsync(acctgTransTypes);
            await context.SaveChangesAsync();
        }

        // gl fiscal type
        if (!context.GlFiscalTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/gl_fiscal_types.json");
            var jsonData = File.ReadAllText(path);

            var glFiscalTypes = JsonConvert.DeserializeObject<List<GlFiscalType>>(jsonData);
            await context.GlFiscalTypes.AddRangeAsync(glFiscalTypes);
            await context.SaveChangesAsync();
        }

        // product average cost types
        if (!context.ProductAverageCostTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_average_cost_types.json");
            var jsonData = File.ReadAllText(path);

            var productAverageCostTypes = JsonConvert.DeserializeObject<List<ProductAverageCostType>>(jsonData);
            await context.ProductAverageCostTypes.AddRangeAsync(productAverageCostTypes);
            await context.SaveChangesAsync();
        }


        // payment gl account type map
        if (!context.PaymentGlAccountTypeMaps.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/payment_gl_account_type_maps.json");
            var jsonData = File.ReadAllText(path);

            var paymentGlAccountTypeMaps = JsonConvert.DeserializeObject<List<PaymentGlAccountTypeMap>>(jsonData);
            await context.PaymentGlAccountTypeMaps.AddRangeAsync(paymentGlAccountTypeMaps);
            await context.SaveChangesAsync();
        }

        // gl account type default
        if (!context.GlAccountTypeDefaults.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/gl_account_type_defaults.json");
            var jsonData = File.ReadAllText(path);

            var glAccountTypeDefaults = JsonConvert.DeserializeObject<List<GlAccountTypeDefault>>(jsonData);
            await context.GlAccountTypeDefaults.AddRangeAsync(glAccountTypeDefaults);
            await context.SaveChangesAsync();
        }

        // invoice item type map
        if (!context.InvoiceItemTypeMaps.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/invoice_item_type_maps.json");
            var jsonData = File.ReadAllText(path);

            var invoiceItemTypeMaps = JsonConvert.DeserializeObject<List<InvoiceItemTypeMap>>(jsonData);
            await context.InvoiceItemTypeMaps.AddRangeAsync(invoiceItemTypeMaps);
            await context.SaveChangesAsync();
        }

        // tax authorities
        if (!context.TaxAuthorities.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/tax_authorities.json");
            var jsonData = File.ReadAllText(path);

            var taxAuthorities = JsonConvert.DeserializeObject<List<TaxAuthority>>(jsonData);
            await context.TaxAuthorities.AddRangeAsync(taxAuthorities);
            await context.SaveChangesAsync();
        }

        // product stores
        if (!context.ProductStores.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_stores.json");
            var jsonData = File.ReadAllText(path);

            var productStores = JsonConvert.DeserializeObject<List<ProductStore>>(jsonData);
            await context.ProductStores.AddRangeAsync(productStores);
            await context.SaveChangesAsync();
        }


        // payment gateway config types
        if (!context.PaymentGatewayConfigTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/payment_gateway_config_types.json");
            var jsonData = File.ReadAllText(path);

            var paymentGatewayConfigTypes = JsonConvert.DeserializeObject<List<PaymentGatewayConfigType>>(jsonData);
            await context.PaymentGatewayConfigTypes.AddRangeAsync(paymentGatewayConfigTypes);
            await context.SaveChangesAsync();
        }

        // payment gateway configs
        if (!context.PaymentGatewayConfigs.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/payment_gateway_configs.json");
            var jsonData = File.ReadAllText(path);

            var paymentGatewayConfigs = JsonConvert.DeserializeObject<List<PaymentGatewayConfig>>(jsonData);
            await context.PaymentGatewayConfigs.AddRangeAsync(paymentGatewayConfigs);
            await context.SaveChangesAsync();
        }

        // product store payment settings
        if (!context.ProductStorePaymentSettings.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/productStorePaymentSettings.json");
            var jsonData = File.ReadAllText(path);

            var productStorePaymentSettings = JsonConvert.DeserializeObject<List<ProductStorePaymentSetting>>(jsonData);
            await context.ProductStorePaymentSettings.AddRangeAsync(productStorePaymentSettings);
            await context.SaveChangesAsync();
        }


        // product store facilities
        if (!context.ProductStoreFacilities.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_store_facilities.json");
            var jsonData = File.ReadAllText(path);

            var productStoreFacilities = JsonConvert.DeserializeObject<List<ProductStoreFacility>>(jsonData);
            await context.ProductStoreFacilities.AddRangeAsync(productStoreFacilities);
            await context.SaveChangesAsync();
        }


        // tax authority categories
        /*if (!context.TaxAuthorityCategories.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/tax_authority_categories_clothes.json");
            var jsonData = File.ReadAllText(path);

            var taxAuthorityCategories = JsonConvert.DeserializeObject<List<TaxAuthorityCategory>>(jsonData);
            await context.TaxAuthorityCategories.AddRangeAsync(taxAuthorityCategories);
            await context.SaveChangesAsync();
        }*/

        // tax authority rate types
        if (!context.TaxAuthorityRateTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/tax_authority_rate_types.json");
            var jsonData = File.ReadAllText(path);

            var taxAuthorityRateTypes = JsonConvert.DeserializeObject<List<TaxAuthorityRateType>>(jsonData);
            await context.TaxAuthorityRateTypes.AddRangeAsync(taxAuthorityRateTypes);
            await context.SaveChangesAsync();
        }

        // tax authority rate products
        if (!context.TaxAuthorityRateProducts.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/tax_authority_rate_products.json");
            var jsonData = File.ReadAllText(path);

            var taxAuthorityRateProducts = JsonConvert.DeserializeObject<List<TaxAuthorityRateProduct>>(jsonData);
            await context.TaxAuthorityRateProducts.AddRangeAsync(taxAuthorityRateProducts);
            await context.SaveChangesAsync();
        }

        // product promos
        if (!context.ProductPromos.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_promos.json");
            var jsonData = File.ReadAllText(path);

            var productPromos = JsonConvert.DeserializeObject<List<ProductPromo>>(jsonData);
            await context.ProductPromos.AddRangeAsync(productPromos);
            await context.SaveChangesAsync();
        }

        // product promo Rules
        if (!context.ProductPromoRules.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_promo_rules.json");
            var jsonData = File.ReadAllText(path);

            var productPromoRules = JsonConvert.DeserializeObject<List<ProductPromoRule>>(jsonData);
            await context.ProductPromoRules.AddRangeAsync(productPromoRules);
            await context.SaveChangesAsync();
        }

        // product promo Actions
        if (!context.ProductPromoActions.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_promo_actions.json");
            var jsonData = File.ReadAllText(path);

            var productPromoActions = JsonConvert.DeserializeObject<List<ProductPromoAction>>(jsonData);
            await context.ProductPromoActions.AddRangeAsync(productPromoActions);
            await context.SaveChangesAsync();
        }

        // product promo conds
        if (!context.ProductPromoConds.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_promo_conds.json");
            var jsonData = File.ReadAllText(path);

            var productPromoConds = JsonConvert.DeserializeObject<List<ProductPromoCond>>(jsonData);
            await context.ProductPromoConds.AddRangeAsync(productPromoConds);
            await context.SaveChangesAsync();
        }

        // tax auth gl accounts
        if (!context.TaxAuthorityGlAccounts.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/tax_authority_gl_accounts.json");
            var jsonData = File.ReadAllText(path);

            var taxAuthorityGlAccounts = JsonConvert.DeserializeObject<List<TaxAuthorityGlAccount>>(jsonData);
            await context.TaxAuthorityGlAccounts.AddRangeAsync(taxAuthorityGlAccounts);
            await context.SaveChangesAsync();
        }

        // fixed asset types
        if (!context.FixedAssetTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/fixed_asset_types_medications.json");
            var jsonData = File.ReadAllText(path);

            var fixedAssetTypes = JsonConvert.DeserializeObject<List<FixedAssetType>>(jsonData);
            await context.FixedAssetTypes.AddRangeAsync(fixedAssetTypes);
            await context.SaveChangesAsync();
        }

        // fixed assets
        if (!context.FixedAssets.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/fixed_assets_medications.json");
            var jsonData = File.ReadAllText(path);

            var fixedAssets = JsonConvert.DeserializeObject<List<FixedAsset>>(jsonData);
            await context.FixedAssets.AddRangeAsync(fixedAssets);
            await context.SaveChangesAsync();
        }

        // Payment Methods
        if (!context.PaymentMethods.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/payment_methods.json");
            var jsonData = File.ReadAllText(path);

            var paymentMethods = JsonConvert.DeserializeObject<List<PaymentMethod>>(jsonData);
            await context.PaymentMethods.AddRangeAsync(paymentMethods);
            await context.SaveChangesAsync();
        }

        // Payment Group Types
        if (!context.PaymentGroupTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/payment_group_types.json");
            var jsonData = File.ReadAllText(path);

            var paymentGroupTypes = JsonConvert.DeserializeObject<List<PaymentGroupType>>(jsonData);
            await context.PaymentGroupTypes.AddRangeAsync(paymentGroupTypes);
            await context.SaveChangesAsync();
        }
        
        // Payment Group
        if (!context.PaymentGroups.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/payment_groups.json");
            var jsonData = File.ReadAllText(path);

            var paymentGroups = JsonConvert.DeserializeObject<List<PaymentGroup>>(jsonData);
            await context.PaymentGroups.AddRangeAsync(paymentGroups);
            await context.SaveChangesAsync();
        }

        // variance reasons
        if (!context.VarianceReasons.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/variance_reasons.json");
            var jsonData = File.ReadAllText(path);

            var varianceReasons = JsonConvert.DeserializeObject<List<VarianceReason>>(jsonData);
            await context.VarianceReasons.AddRangeAsync(varianceReasons);
            await context.SaveChangesAsync();
        }

        // variance reasons gl accounts
        if (!context.VarianceReasonGlAccounts.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/variance_reason_gl_accounts.json");
            var jsonData = File.ReadAllText(path);

            var varianceReasonGlAccounts = JsonConvert.DeserializeObject<List<VarianceReasonGlAccount>>(jsonData);
            await context.VarianceReasonGlAccounts.AddRangeAsync(varianceReasonGlAccounts);
            await context.SaveChangesAsync();
        }

        // Shipment Method Types
        if (!context.ShipmentMethodTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/shipment_method_types.json");
            var jsonData = File.ReadAllText(path);

            var shipmentMethodTypes = JsonConvert.DeserializeObject<List<ShipmentMethodType>>(jsonData);
            await context.ShipmentMethodTypes.AddRangeAsync(shipmentMethodTypes);
            await context.SaveChangesAsync();
        }

        // workeffort types
        if (!context.WorkEffortTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/workeffort_types.json");
            var jsonData = File.ReadAllText(path);

            var workEffortTypes = JsonConvert.DeserializeObject<List<WorkEffortType>>(jsonData);
            await context.WorkEffortTypes.AddRangeAsync(workEffortTypes);
            await context.SaveChangesAsync();
        }


        // workeffort purpose types
        if (!context.WorkEffortPurposeTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/workeffort_purpose_types.json");
            var jsonData = File.ReadAllText(path);

            var workEffortPurposeTypes = JsonConvert.DeserializeObject<List<WorkEffortPurposeType>>(jsonData);
            await context.WorkEffortPurposeTypes.AddRangeAsync(workEffortPurposeTypes);
            await context.SaveChangesAsync();
        }


        // workefforts
        /*if (!context.WorkEfforts.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/workefforts_clothes.json");
            var jsonData = File.ReadAllText(path);

            var workEfforts = JsonConvert.DeserializeObject<List<WorkEffort>>(jsonData);
            await context.WorkEfforts.AddRangeAsync(workEfforts);
            await context.SaveChangesAsync();
        }*/

        // workeffort party assignments
        /*if (!context.WorkEffortPartyAssignments.Any())
        {
            var roleTypes1 = new[] { "OPERATOR", "MACHINE_OPERATOR", "ASSEMBLY_OPERATOR", "PACKAGING_OPERATOR" };
            var routingTasks = await context.WorkEfforts.Where(we => we.WorkEffortTypeId == "ROU_TASK").ToListAsync();
            var partyRoles2 = await context.PartyRoles.Where(pr => roleTypes1.Contains(pr.RoleTypeId)).ToListAsync();
            var random = new Random();

            // Limit the number of assignments to 3
            int maxAssignments =
                Math.Min(3, partyRoles2.Count); // Ensure we don't go over the number of partyRoles available
            var selectedPartyRoles =
                partyRoles2.OrderBy(x => random.Next()).Take(maxAssignments).ToList(); // Randomly select up to 3 roles

            foreach (var partyRole in selectedPartyRoles)
            {
                // If there is only one routing task, use it directly, otherwise pick a random one
                var routingTask = routingTasks.Count == 1
                    ? routingTasks.First()
                    : routingTasks[random.Next(routingTasks.Count)];

                var fakeWorkEffortPartyAssignment = new Faker<WorkEffortPartyAssignment>()
                    .RuleFor(o => o.WorkEffortId, f => routingTask.WorkEffortId)
                    .RuleFor(o => o.PartyId, f => partyRole.PartyId)
                    .RuleFor(o => o.RoleTypeId, f => partyRole.RoleTypeId)
                    .RuleFor(o => o.FromDate, f => nowDateTime)
                    .RuleFor(o => o.ThruDate, f => (DateTime?)null)
                    .RuleFor(o => o.AssignedByUserLoginId, f => (string)null)
                    .RuleFor(o => o.StatusId, f => "PRTYASGN_OFFERED")
                    .RuleFor(o => o.StatusDateTime, f => (DateTime?)null)
                    .RuleFor(o => o.ExpectationEnumId, f => (string)null)
                    .RuleFor(o => o.DelegateReasonEnumId, f => (string)null)
                    .RuleFor(o => o.FacilityId, f => (string)null)
                    .RuleFor(o => o.Comments, f => (string)null)
                    .RuleFor(o => o.MustRsvp, f => (string)null)
                    .RuleFor(o => o.AvailabilityStatusId, f => "WEPA_AV_AVAILABLE")
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedTxStamp, f => nowDateTime)
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.CreatedTxStamp, f => nowDateTime);

                await context.WorkEffortPartyAssignments.AddAsync(fakeWorkEffortPartyAssignment);
            }

            await context.SaveChangesAsync();
        }
        */


        // cost component types
        if (!context.CostComponentTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/cost_component_types.json");
            var jsonData = File.ReadAllText(path);

            var costComponentTypes = JsonConvert.DeserializeObject<List<CostComponentType>>(jsonData);
            await context.CostComponentTypes.AddRangeAsync(costComponentTypes);
            await context.SaveChangesAsync();
        }

        // cost component calcs
        if (!context.CostComponentCalcs.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/cost_component_calcs.json");
            var jsonData = File.ReadAllText(path);

            var costComponentCalcs = JsonConvert.DeserializeObject<List<CostComponentCalc>>(jsonData);
            await context.CostComponentCalcs.AddRangeAsync(costComponentCalcs);
            await context.SaveChangesAsync();
        }

        // cost component calcs
        /*if (!context.CostComponents.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/cost_components_medications.json");
            var jsonData = File.ReadAllText(path);

            var costComponents = JsonConvert.DeserializeObject<List<CostComponent>>(jsonData);
            await context.CostComponents.AddRangeAsync(costComponents);
            await context.SaveChangesAsync();
        }*/

        // fixed asset standard costs types
        if (!context.FixedAssetStdCostTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/fixed_asset_std_cost_types.json");
            var jsonData = File.ReadAllText(path);

            var fixedAssetStdCostTypes = JsonConvert.DeserializeObject<List<FixedAssetStdCostType>>(jsonData);
            await context.FixedAssetStdCostTypes.AddRangeAsync(fixedAssetStdCostTypes);
            await context.SaveChangesAsync();
        }


        // fixed asset standard costs
        /*if (!context.FixedAssetStdCosts.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/fixed_asset_std_costs.json");
            var jsonData = File.ReadAllText(path);

            var fixedAssetStdCosts = JsonConvert.DeserializeObject<List<FixedAssetStdCost>>(jsonData);
            await context.FixedAssetStdCosts.AddRangeAsync(fixedAssetStdCosts);
            await context.SaveChangesAsync();
        }*/

        // workEffort Assoc Types
        if (!context.WorkEffortAssocTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/workeffort_assoc_types.json");
            var jsonData = File.ReadAllText(path);

            var workEffortAssocTypes = JsonConvert.DeserializeObject<List<WorkEffortAssocType>>(jsonData);
            await context.WorkEffortAssocTypes.AddRangeAsync(workEffortAssocTypes);
            await context.SaveChangesAsync();
        }

        // workEffort Assocs
        /*if (!context.WorkEffortAssocs.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/workeffort_assocs_clothes.json");
            var jsonData = File.ReadAllText(path);

            var workEffortAssocs = JsonConvert.DeserializeObject<List<WorkEffortAssoc>>(jsonData);
            await context.WorkEffortAssocs.AddRangeAsync(workEffortAssocs);
            await context.SaveChangesAsync();
        }*/

        // workEffort good standard types
        if (!context.WorkEffortGoodStandardTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/workeffort_good_standard_types.json");
            var jsonData = File.ReadAllText(path);

            var workEffortGoodStandardTypes = JsonConvert.DeserializeObject<List<WorkEffortGoodStandardType>>(jsonData);
            await context.WorkEffortGoodStandardTypes.AddRangeAsync(workEffortGoodStandardTypes);
            await context.SaveChangesAsync();
        }

        // workEffort good standards
        /*if (!context.WorkEffortGoodStandards.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/workeffort_good_standards_clothes.json");
            var jsonData = File.ReadAllText(path);

            var workEffortGoodStandards = JsonConvert.DeserializeObject<List<WorkEffortGoodStandard>>(jsonData);
            await context.WorkEffortGoodStandards.AddRangeAsync(workEffortGoodStandards);
            await context.SaveChangesAsync();
        }
        */

        // WorkEffort cost calcs
        /*if (!context.WorkEffortCostCalcs.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/workeffort_cost_calcs_clothes.json");
            var jsonData = File.ReadAllText(path);

            var workEffortCostCalcs = JsonConvert.DeserializeObject<List<WorkEffortCostCalc>>(jsonData);
            await context.WorkEffortCostCalcs.AddRangeAsync(workEffortCostCalcs);
            await context.SaveChangesAsync();
        }*/

        // Product cost components calc
        /*if (!context.ProductCostComponentCalcs.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_cost_component_calcs.json");
            var jsonData = File.ReadAllText(path);

            var productCostComponentCalcs = JsonConvert.DeserializeObject<List<ProductCostComponentCalc>>(jsonData);
            await context.ProductCostComponentCalcs.AddRangeAsync(productCostComponentCalcs);
            await context.SaveChangesAsync();
        }*/

        // Tech Data Calendar Week
        if (!context.TechDataCalendarWeeks.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/tech_data_calendar_weeks.json");
            var jsonData = File.ReadAllText(path);

            var techDataCalendarWeeeks = JsonConvert.DeserializeObject<List<TechDataCalendarWeek>>(jsonData);
            await context.TechDataCalendarWeeks.AddRangeAsync(techDataCalendarWeeeks);
            await context.SaveChangesAsync();
        }

        // Tech Data Calendar
        if (!context.TechDataCalendars.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/tech_data_calendars.json");
            var jsonData = File.ReadAllText(path);

            var techDataCalendars = JsonConvert.DeserializeObject<List<TechDataCalendar>>(jsonData);
            await context.TechDataCalendars.AddRangeAsync(techDataCalendars);
            await context.SaveChangesAsync();
        }


        // Tech Data Calendar exception days
        if (!context.TechDataCalendarExcDays.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/tech_data_calendar_exc_days.json");
            var jsonData = File.ReadAllText(path);

            var techDataCalendarExcDays = JsonConvert.DeserializeObject<List<TechDataCalendarExcDay>>(jsonData);
            await context.TechDataCalendarExcDays.AddRangeAsync(techDataCalendarExcDays);
            await context.SaveChangesAsync();
        }
        
        /*if (!context.InventoryItems.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/inventory_items.json");
            var jsonData = File.ReadAllText(path);

            var inventoryItems = JsonConvert.DeserializeObject<List<InventoryItem>>(jsonData);
            await context.InventoryItems.AddRangeAsync(inventoryItems);
            await context.SaveChangesAsync();
        }*/

        // Seeding SupplierProducts
        /*
        if (!context.SupplierProducts.Any())
        {
            // Fetching products where ProductTypeId is 'RAW_MATERIAL'
            var rawMaterials = context.Products
                .Where(x => x.ProductTypeId == "RAW_MATERIAL")
                .ToList();

            // Fetching parties where MainRole is 'SUPPLIER'
            var suppliers = context.Parties
                .Where(p => p.MainRole == "SUPPLIER")
                .ToList();

            // Using Faker to generate consistent random values
            var faker = new Faker();
            decimal basePrice = faker.Random.Decimal(15, 25);
            int baseMinimumOrderQuantity = faker.Random.Int(1, 5);
            DateTime baseAvailableFromDate = faker.Date.RecentOffset(30, nowDateTime).DateTime;

            foreach (var supplier in suppliers)
            {
                foreach (var product in rawMaterials)
                {
                    var fakeProductSupplier = new SupplierProduct
                    {
                        PartyId = supplier.PartyId,
                        ProductId = product.ProductId,
                        LastPrice = basePrice + faker.Random.Decimal(-2, 2), // Slight variation in price
                        SupplierPrefOrderId = "10_MAIN_SUPPL",
                        CurrencyUomId = "EGP",
                        MinimumOrderQuantity =
                            baseMinimumOrderQuantity + faker.Random.Int(-1, 1), // Slight variation in quantity
                        AvailableFromDate =
                            baseAvailableFromDate.AddDays(faker.Random.Int(-5, 5)), // Slight variation in date
                        CreatedStamp = nowDateTime,
                        LastUpdatedStamp = nowDateTime
                    };

                    context.SupplierProducts.Add(fakeProductSupplier);
                }
            }

            await context.SaveChangesAsync();
        }
        */

        // Seeding InventoryItems and InventoryItemDetails
        /*if (!context.InventoryItems.Any())
        {
            // Using Faker to generate consistent random values
            var faker = new Faker();
            int inventoryItemIdStart = 500;
            int inventoryItemDetailSeqIdStart = 500;

            // The current date/time for stamping
            nowDateTime = DateTime.UtcNow;

            // -- 1. RAW MATERIALS --

            // 1a. Fetch products where ProductTypeId is 'RAW_MATERIAL'
            var rawMaterials = context.Products
                .Where(p => p.ProductTypeId == "RAW_MATERIAL")
                .ToList();

            // 1b. Fetch parties where MainRole is 'SUPPLIER'
            var suppliers = context.Parties
                .Where(p => p.MainRole == "SUPPLIER")
                .ToList();

            // Shuffle the supplier list to randomize picking
            var shuffledSuppliers = suppliers.OrderBy(s => faker.Random.Int()).ToList();

            // 1c. Fetch facility locations for raw materials facility
            //     Splitting out BULK and PICKLOC explicitly
            var rawMaterialBulkLocations = context.FacilityLocations
                .Where(fl =>
                    fl.FacilityId == "b6705327-bb0b-421f-9a1e-e94bbf7a68d2"
                    && fl.LocationTypeEnumId == "FLT_BULK")
                .Select(fl => fl.LocationSeqId)
                .ToList();

            var rawMaterialPickLocations = context.FacilityLocations
                .Where(fl =>
                    fl.FacilityId == "b6705327-bb0b-421f-9a1e-e94bbf7a68d2"
                    && fl.LocationTypeEnumId == "FLT_PICKLOC")
                .Select(fl => fl.LocationSeqId)
                .ToList();

            // 1d. Create inventory items for each raw material
            foreach (var product in rawMaterials)
            {
                // Select up to two suppliers for the current product
                var selectedSuppliers = shuffledSuppliers.Take(2).ToList();

                foreach (var supplier in selectedSuppliers)
                {
                    // Generate a base integer quantity
                    var baseQuantity = faker.Random.Int(190, 210);

                    // Fetch the unit cost from ProductPrices table (DEFAULT_PRICE)
                    var unitCost = context.ProductPrices
                                       .Where(pp =>
                                           pp.ProductId == product.ProductId &&
                                           pp.ProductPriceTypeId == "DEFAULT_PRICE")
                                       .Select(pp => pp.Price)
                                       .FirstOrDefault()
                                   ?? faker.Random.Decimal(8, 10);

                    // We want to insert two items: BULK and PICKLOC
                    // We'll define the pairs of location lists to handle in a small collection
                    var locationPairs = new List<(string LocationType, List<string> Locations)>
                    {
                        ("FLT_BULK", rawMaterialBulkLocations),
                        ("FLT_PICKLOC", rawMaterialPickLocations)
                    };

                    foreach (var (locationType, locations) in locationPairs)
                    {
                        // Pick a random location from the respective list
                        var randomLocationSeqId = locations
                            .OrderBy(_ => faker.Random.Int())
                            .FirstOrDefault();

                        if (string.IsNullOrEmpty(randomLocationSeqId))
                        {
                            // If there is no location of this type, skip to the next
                            continue;
                        }

                        var inventoryItem = new InventoryItem
                        {
                            InventoryItemId = inventoryItemIdStart.ToString(),
                            InventoryItemTypeId = "NON_SERIAL_INV_ITEM",
                            ProductId = product.ProductId,
                            PartyId = supplier.PartyId,
                            OwnerPartyId = "Company",
                            FacilityId = "b6705327-bb0b-421f-9a1e-e94bbf7a68d2", // Raw Materials Facility
                            BinNumber = "1",
                            LocationSeqId = randomLocationSeqId,
                            QuantityOnHandTotal = baseQuantity,
                            AvailableToPromiseTotal = baseQuantity,
                            AccountingQuantityTotal = baseQuantity,
                            UnitCost = unitCost,
                            CurrencyUomId = "EGP",
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        };

                        context.InventoryItems.Add(inventoryItem);

                        var inventoryItemDetail = new InventoryItemDetail
                        {
                            InventoryItemId = inventoryItem.InventoryItemId,
                            InventoryItemDetailSeqId = inventoryItemDetailSeqIdStart.ToString(),
                            QuantityOnHandDiff = baseQuantity,
                            AvailableToPromiseDiff = baseQuantity,
                            AccountingQuantityDiff = baseQuantity,
                            EffectiveDate = nowDateTime,
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        };

                        context.InventoryItemDetails.Add(inventoryItemDetail);

                        // Increment counters for each record
                        inventoryItemIdStart++;
                        inventoryItemDetailSeqIdStart++;
                    }
                }
            }

            await context.SaveChangesAsync();

            // -- 2. FINISHED GOODS --

            // 2a. Fetch products where ProductTypeId is 'FINISHED_GOOD'
            var finishedGoods = context.Products
                .Where(p => p.ProductTypeId == "FINISHED_GOOD")
                .ToList();

            // 2b. Fetch facility locations for finished goods facility
            var finishedGoodsBulkLocations = context.FacilityLocations
                .Where(fl =>
                    fl.FacilityId == "a5826c99-ca43-4114-9496-0acf1ed71049"
                    && fl.LocationTypeEnumId == "FLT_BULK")
                .Select(fl => fl.LocationSeqId)
                .ToList();

            var finishedGoodsPickLocations = context.FacilityLocations
                .Where(fl =>
                    fl.FacilityId == "a5826c99-ca43-4114-9496-0acf1ed71049"
                    && fl.LocationTypeEnumId == "FLT_PICKLOC")
                .Select(fl => fl.LocationSeqId)
                .ToList();

            // 2c. Create inventory items for each finished good
            foreach (var product in finishedGoods)
            {
                // Generate a base integer quantity
                var baseQuantity = faker.Random.Int(190, 210);

                // Fetch the unit cost from ProductPrices table (DEFAULT_PRICE)
                var unitCost = context.ProductPrices
                                   .Where(pp =>
                                       pp.ProductId == product.ProductId && pp.ProductPriceTypeId == "DEFAULT_PRICE")
                                   .Select(pp => pp.Price)
                                   .FirstOrDefault()
                               ?? faker.Random.Decimal(8, 10);

                // Insert two items: BULK and PICKLOC
                var locationPairs = new List<(string LocationType, List<string> Locations)>
                {
                    ("FLT_BULK", finishedGoodsBulkLocations),
                    ("FLT_PICKLOC", finishedGoodsPickLocations)
                };

                foreach (var (locationType, locations) in locationPairs)
                {
                    // Pick a random location from the respective list
                    var randomLocationSeqId = locations
                        .OrderBy(_ => faker.Random.Int())
                        .FirstOrDefault();

                    if (string.IsNullOrEmpty(randomLocationSeqId))
                    {
                        // If there is no location of this type, skip to the next
                        continue;
                    }

                    var inventoryItem = new InventoryItem
                    {
                        InventoryItemId = inventoryItemIdStart.ToString(),
                        InventoryItemTypeId = "NON_SERIAL_INV_ITEM",
                        ProductId = product.ProductId,
                        OwnerPartyId = "Company",
                        FacilityId = "a5826c99-ca43-4114-9496-0acf1ed71049", // Finished Goods Facility
                        BinNumber = "1",
                        LocationSeqId = randomLocationSeqId,
                        QuantityOnHandTotal = baseQuantity,
                        AvailableToPromiseTotal = baseQuantity,
                        AccountingQuantityTotal = baseQuantity,
                        UnitCost = unitCost,
                        CurrencyUomId = "EGP",
                        CreatedStamp = nowDateTime,
                        LastUpdatedStamp = nowDateTime
                    };

                    context.InventoryItems.Add(inventoryItem);

                    var inventoryItemDetail = new InventoryItemDetail
                    {
                        InventoryItemId = inventoryItem.InventoryItemId,
                        InventoryItemDetailSeqId = inventoryItemDetailSeqIdStart.ToString(),
                        QuantityOnHandDiff = baseQuantity,
                        AvailableToPromiseDiff = baseQuantity,
                        AccountingQuantityDiff = baseQuantity,
                        EffectiveDate = nowDateTime,
                        CreatedStamp = nowDateTime,
                        LastUpdatedStamp = nowDateTime
                    };

                    context.InventoryItemDetails.Add(inventoryItemDetail);

                    // Increment counters for each record
                    inventoryItemIdStart++;
                    inventoryItemDetailSeqIdStart++;
                }
            }

            await context.SaveChangesAsync();
        }
        */


//---------------------------------Product Promo---------------------------------
        /*
        var categoryCounts = new Dictionary<string, int>();
        var productPromoIds = new List<string>();

        // Step 1 & 2: Populate the dictionary
        foreach (var product in context.Products.Where(p => p.ProductTypeId == "FINISHED_GOOD"))
        {
            if (categoryCounts.ContainsKey(product.PrimaryProductCategoryId))
                categoryCounts[product.PrimaryProductCategoryId]++;
            else
                categoryCounts[product.PrimaryProductCategoryId] = 1;
        }

        // Step 3 & 4: Gather the ProductIds
        foreach (var product in context.Products.Where(p => p.ProductTypeId == "FINISHED_GOOD"))
            if (categoryCounts[product.PrimaryProductCategoryId] >= 2)
                productPromoIds.Add(product.ProductId);

        // Print the results
        foreach (var id in productPromoIds) Console.WriteLine(id);


        // product promo product
        if (!context.ProductPromoProducts.Any())
        {
            var productPromoProducts = new List<ProductPromoProduct>
            {
                new()
                {
                    ProductPromoId = "9015",
                    ProductPromoRuleId = "01",
                    ProductId = productPromoIds[2],
                    ProductPromoActionSeqId = "01",
                    ProductPromoCondSeqId = "01",
                    ProductPromoApplEnumId = "PPPA_INCLUDE",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.ProductPromoProducts.AddRangeAsync(productPromoProducts);

            // get productPromoAction for this productPromoId
            var productPromoAction = context.ProductPromoActions.FirstOrDefault(x => x.ProductPromoId == "9015");
            if (productPromoAction != null)
            {
                productPromoAction.ProductId = productPromoIds[0];
                context.ProductPromoActions.Update(productPromoAction);
            }

            await context.SaveChangesAsync();
        }

        // product promo categories
        if (!context.ProductPromoCategories.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_promo_categories.json");
            var jsonData = File.ReadAllText(path);

            var productPromoCategories = JsonConvert.DeserializeObject<List<ProductPromoCategory>>(jsonData);
            await context.ProductPromoCategories.AddRangeAsync(productPromoCategories);
            await context.SaveChangesAsync();
        }

        // product promo codes
        if (!context.ProductPromoCodes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_promo_codes.json");
            var jsonData = File.ReadAllText(path);

            var productPromoCodes = JsonConvert.DeserializeObject<List<ProductPromoCode>>(jsonData);
            await context.ProductPromoCodes.AddRangeAsync(productPromoCodes);
            await context.SaveChangesAsync();
        }

        // product store promo appls
        if (!context.ProductStorePromoAppls.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_store_promo_appls.json");
            var jsonData = File.ReadAllText(path);

            var productStorePromoAppls = JsonConvert.DeserializeObject<List<ProductStorePromoAppl>>(jsonData);
            await context.ProductStorePromoAppls.AddRangeAsync(productStorePromoAppls);
            await context.SaveChangesAsync();
        }
        */


        // party tax auth infos
        if (!context.PartyTaxAuthInfos.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/party_tax_auth_infos.json");
            var jsonData = File.ReadAllText(path);

            var partyTaxAuthInfos = JsonConvert.DeserializeObject<List<PartyTaxAuthInfo>>(jsonData);
            await context.PartyTaxAuthInfos.AddRangeAsync(partyTaxAuthInfos);

            var customerId = context.Parties
                .Where(a => a.MainRole == "CUSTOMER")
                .Skip(1)
                .FirstOrDefault()?.PartyId;
            // create party tax auth infos for customer if exists with 
            // isExempt is True
            if (customerId != null)
            {
                var partyTaxAuthInfosForCustomer = new List<PartyTaxAuthInfo>
                {
                    new()
                    {
                        PartyId = customerId,
                        TaxAuthGeoId = "EGY",
                        TaxAuthPartyId = "EgyptTaxAuth",
                        FromDate = nowDateTime,
                        IsExempt = "Y",
                        LastUpdatedStamp = nowDateTime,
                        CreatedStamp = nowDateTime
                    }
                };
                await context.PartyTaxAuthInfos.AddRangeAsync(partyTaxAuthInfosForCustomer);
                await context.SaveChangesAsync();
            }
        }

        

        

        // accounting transaction
        if (!context.AcctgTrans.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/acctg_trans.json");
            var jsonData = File.ReadAllText(path);

            var acctgTrans = JsonConvert.DeserializeObject<List<AcctgTran>>(jsonData);
            await context.AcctgTrans.AddRangeAsync(acctgTrans);
            await context.SaveChangesAsync();
        }

        // accounting transaction entries
        if (!context.AcctgTransEntries.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/acctg_trans_entries.json");
            var jsonData = File.ReadAllText(path);

            var acctgTransEntries = JsonConvert.DeserializeObject<List<AcctgTransEntry>>(jsonData);
            await context.AcctgTransEntries.AddRangeAsync(acctgTransEntries);
            await context.SaveChangesAsync();
        }

        //Party Gl Accounts
        if (!context.PartyGlAccounts.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/party_gl_accounts.json");
            var jsonData = File.ReadAllText(path);

            var partyGlAccounts = JsonConvert.DeserializeObject<List<PartyGlAccount>>(jsonData);
            await context.PartyGlAccounts.AddRangeAsync(partyGlAccounts);
            await context.SaveChangesAsync();
        }
        
        //Party classification types
        if (!context.PartyClassificationTypes.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/party_classification_types.json");
            var jsonData = File.ReadAllText(path);

            var partyClassificationTypes = JsonConvert.DeserializeObject<List<PartyClassificationType>>(jsonData);
            await context.PartyClassificationTypes.AddRangeAsync(partyClassificationTypes);
            await context.SaveChangesAsync();
        }
        
        //Party classification groups
        if (!context.PartyClassificationGroups.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/party_classification_groups.json");
            var jsonData = File.ReadAllText(path);

            var partyClassificationGroups = JsonConvert.DeserializeObject<List<PartyClassificationGroup>>(jsonData);
            await context.PartyClassificationGroups.AddRangeAsync(partyClassificationGroups);
            await context.SaveChangesAsync();
        }
        
        //quality standards
        if (!context.QualityStandards.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/quality_standards.json");
            var jsonData = File.ReadAllText(path);

            var qualityStandards = JsonConvert.DeserializeObject<List<QualityStandard>>(jsonData);
            await context.QualityStandards.AddRangeAsync(qualityStandards);
            await context.SaveChangesAsync();
        }
        
        //product quality standards
        /*if (!context.ProductQualityStandards.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_quality_standards.json");
            var jsonData = File.ReadAllText(path);

            var productQualityStandards = JsonConvert.DeserializeObject<List<ProductQualityStandard>>(jsonData);
            await context.ProductQualityStandards.AddRangeAsync(productQualityStandards);
            await context.SaveChangesAsync();
        }*/
        
         //Transaction type account rules
        if (!context.TransactionTypeAccountRules.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/transaction_type_account_rules.json");
            var jsonData = File.ReadAllText(path);

            var transactionTypeAccountRules = JsonConvert.DeserializeObject<List<TransactionTypeAccountRule>>(jsonData);
            await context.TransactionTypeAccountRules.AddRangeAsync(transactionTypeAccountRules);
            await context.SaveChangesAsync();
        }
        
        /*if (!context.SupplierProducts.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/supplier_products.json");
            var jsonData = File.ReadAllText(path);

            var supplierProducts = JsonConvert.DeserializeObject<List<SupplierProduct>>(jsonData);
            await context.SupplierProducts.AddRangeAsync(supplierProducts);
            await context.SaveChangesAsync();
        }*/
        
        if (!context.UserLogins.Any())
        {
            // Create a list of UserLogin records with PartyId used for both PartyId and UserLoginId.
            var userLogins = new List<UserLogin>
            {
                new UserLogin
                {
                    UserLoginId = "3bb4e859-1157-4cc7-81b5-10f419359a41",
                    PartyId = "3bb4e859-1157-4cc7-81b5-10f419359a41",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new UserLogin
                {
                    UserLoginId = "29a02dc0-70ea-46d0-a687-6a72b2f91d07",
                    PartyId = "29a02dc0-70ea-46d0-a687-6a72b2f91d07",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new UserLogin
                {
                    UserLoginId = "4cdcda5a-7eee-4845-af3c-51d02ad4cb28",
                    PartyId = "4cdcda5a-7eee-4845-af3c-51d02ad4cb28",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new UserLogin
                {
                    UserLoginId = "62ddd58f-912d-43f9-a112-8ca6b98f075e",
                    PartyId = "62ddd58f-912d-43f9-a112-8ca6b98f075e",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                }
            };

            // Add all records to the UserLogin table and persist changes.
            context.UserLogins.AddRange(userLogins);
            await context.SaveChangesAsync();
        }


        if (!userManager.Users.Any())
        {
            var users = new List<AppUserLogin>
            {
                new()
                {
                    DisplayName = "Emad Radwan",
                    UserName = "Emad",
                    PartyId = "3bb4e859-1157-4cc7-81b5-10f419359a41",
                    OrganizationPartyId = "Company",
                    ProductStoreId = "9000",
                    Email = "eradwan1967@gmail.com",
                    DualLanguage = "N",
                    EmailConfirmed = true,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    DisplayName = "Youssief Radwan",
                    UserName = "Youssief",
                    PartyId = "29a02dc0-70ea-46d0-a687-6a72b2f91d07",
                    OrganizationPartyId = "Company",
                    ProductStoreId = "9000",
                    Email = "youssefer1997@gmail.com",
                    DualLanguage = "N",
                    EmailConfirmed = true,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    DisplayName = "Shreef Mohamed",
                    UserName = "Shreef",
                    PartyId = "4cdcda5a-7eee-4845-af3c-51d02ad4cb28",
                    OrganizationPartyId = "Company",
                    ProductStoreId = "9000",
                    Email = "Shreef@gmail.com",
                    DualLanguage = "N",
                    EmailConfirmed = true,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    DisplayName = "Turkish User",
                    UserName = "Turkish",
                    PartyId = "62ddd58f-912d-43f9-a112-8ca6b98f075e",
                    OrganizationPartyId = "Company",
                    ProductStoreId = "9000",
                    Email = "turkishUser@gmail.com",
                    DualLanguage = "Y",
                    EmailConfirmed = true,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                }
            };

            foreach (var user in users) await userManager.CreateAsync(user, "Pa$$w0rd");

            // Seed Roles for Emad
            /*var emad = await userManager.FindByEmailAsync("eradwan1967@gmail.com");
            await userManager.AddToRolesAsync(emad,
                new[] { "Member", "AddAdjustments", "AddDiscountAdjustment5", "AddDiscountAdjustment10" });
                            

            // Seed Roles for Youssief
            var youssief = await userManager.FindByEmailAsync("youssefer1997@gmail.com");
            await userManager.AddToRolesAsync(youssief,
                new[] { "Member", "AddAdjustments", "AddDiscountAdjustment5", "AddDiscountAdjustment10" });

            // Seed Roles for Mohamed
            var mohamed = await userManager.FindByEmailAsync("mshehab@gmail.com");
            await userManager.AddToRolesAsync(mohamed,
                new[] { "Member", "AddAdjustments", "AddDiscountAdjustment5" });

            var turkish = await userManager.FindByEmailAsync("turkishUser@gmail.com");
            await userManager.AddToRolesAsync(turkish,
                new[] { "Member", "AddAdjustments", "AddDiscountAdjustment5" });*/
        }

        
    }
}