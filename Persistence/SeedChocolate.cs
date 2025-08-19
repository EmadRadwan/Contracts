using Bogus;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Person = Domain.Person;

namespace Persistence;

public class SeedChocolate
{
    public static async Task SeedData(DataContext context,
        UserManager<AppUserLogin> userManager, RoleManager<ApplicationRole> roleManager)
    {
        var dateNow = DateTime.UtcNow;
        var nowDateTime = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, dateNow.Minute,
            dateNow.Second, 0, DateTimeKind.Utc);
        if (!userManager.Users.Any())
        {
            var users = new List<AppUserLogin>
            {
                new()
                {
                    DisplayName = "Emad Radwan",
                    UserName = "Emad",
                    Email = "eradwan1967@gmail.com",
                    EmailConfirmed = true,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    DisplayName = "Youssief Radwan",
                    UserName = "Youssief",
                    Email = "youssefer1997@gmail.com",
                    EmailConfirmed = true,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    DisplayName = "Mohamed Shehab",
                    UserName = "Mohamed",
                    Email = "mShehab@gmail.com",
                    EmailConfirmed = true,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                }
            };

            foreach (var user in users) await userManager.CreateAsync(user, "Pa$$w0rd");

            // Seed Roles for Emad
            var emad = await userManager.FindByEmailAsync("eradwan1967@gmail.com");
            await userManager.AddToRolesAsync(emad,
                new[] { "Admin", "Member", "AddAdjustments", "AddDiscountAdjustment5", "AddDiscountAdjustment10" });

            // Seed Roles for Youssief
            var youssief = await userManager.FindByEmailAsync("youssefer1997@gmail.com");
            await userManager.AddToRolesAsync(youssief, new[] { "Member" });

            // Seed Roles for Mohamed
            var mohamed = await userManager.FindByEmailAsync("mshehab@gmail.com");
            await userManager.AddToRolesAsync(mohamed, new[] { "Member", "AddAdjustments", "AddDiscountAdjustment5" });
        }

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
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_category_types_Chocolate.json");
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
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_categories_Chocolate.json");
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
        /*if (!context.ProductFeatures.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_features.json");
            var jsonData = File.ReadAllText(path);

            var productFeatures = JsonConvert.DeserializeObject<List<ProductFeature>>(jsonData);
            await context.ProductFeatures.AddRangeAsync(productFeatures);
            await context.SaveChangesAsync();
        }*/

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

        //Party Type 
        if (!context.PartyTypes.Any())
        {
            var partyTypes = new List<PartyType>
            {
                new()
                {
                    PartyTypeId = "PERSON",
                    Description = "Person",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    PartyTypeId = "PARTY_GROUP",
                    Description = "Party Group",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    PartyTypeId = "LEGAL_ORGANIZATION",
                    Description = "Legal Organization",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                }
            };

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


        //ContactMechType
        if (!context.ContactMechTypes.Any())
        {
            var contactMechTypes = new List<ContactMechType>
            {
                new()
                {
                    ContactMechTypeId = "ELECTRONIC_ADDRESS",
                    ParentTypeId = null,
                    Description = "Electronic Address",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    ContactMechTypeId = "EMAIL_ADDRESS",
                    ParentTypeId = "ELECTRONIC_ADDRESS",
                    Description = "Email Address",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    ContactMechTypeId = "POSTAL_ADDRESS",
                    ParentTypeId = null,
                    Description = "Postal Address",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    ContactMechTypeId = "TELECOM_NUMBER",
                    ParentTypeId = null,
                    Description = "Phone Number",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                }
            };

            await context.ContactMechTypes.AddRangeAsync(contactMechTypes);
            await context.SaveChangesAsync();
        }

        //ContactMechPurposeType
        if (!context.ContactMechPurposeTypes.Any())
        {
            var contactMechPurposeType = new List<ContactMechPurposeType>
            {
                new()
                {
                    ContactMechPurposeTypeId = "GENERAL_LOCATION",
                    ParentTypeId = null,
                    Description = "General Correspondence Address",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    ContactMechPurposeTypeId = "PHONE_MOBILE",
                    ParentTypeId = null,
                    Description = "Mobile Phone Number",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    ContactMechPurposeTypeId = "PRIMARY_PHONE",
                    ParentTypeId = null,
                    Description = "Primary Phone Number",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    ContactMechPurposeTypeId = "SHIPPING_LOCATION",
                    ParentTypeId = null,
                    Description = "Shipping Destination Address",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    ContactMechPurposeTypeId = "PRIMARY_EMAIL",
                    ParentTypeId = null,
                    Description = "Primary Email Address",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                }
            };

            await context.ContactMechPurposeTypes.AddRangeAsync(contactMechPurposeType);
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


        // Facility Types
        if (!context.FacilityTypes.Any())
        {
            var facilityTypes = new List<FacilityType>
            {
                new()
                {
                    FacilityTypeId = "BUILDING",
                    Description = "Building"
                },
                new()
                {
                    FacilityTypeId = "WAREHOUSE",
                    Description = "Warehouse"
                },
                new()
                {
                    FacilityTypeId = "ROOM",
                    Description = "Room"
                },
                new()
                {
                    FacilityTypeId = "WORKSHOP",
                    Description = "Workshop"
                }
            };
            await context.FacilityTypes.AddRangeAsync(facilityTypes);
            await context.SaveChangesAsync();
        }

        // Facilities
        var guid6 = Guid.NewGuid().ToString();
        if (!context.Facilities.Any())
        {
            var facilities = new List<Facility>
            {
                new()
                {
                    FacilityId = guid6,
                    FacilityTypeId = "WAREHOUSE",
                    FacilityName = "Warehouse facility # 1"
                },
                new()
                {
                    FacilityId = Guid.NewGuid().ToString(),
                    FacilityTypeId = "WORKSHOP",
                    FacilityName = "Workshop facility # 1"
                }
            };
            await context.Facilities.AddRangeAsync(facilities);
            await context.SaveChangesAsync();
        }

        // Facility Locations
        if (!context.FacilityLocations.Any())
        {
            var facilityLocations = new List<FacilityLocation>
            {
                new()
                {
                    FacilityId = guid6,
                    LocationSeqId = "TLTLTLUL01",
                    LocationTypeEnumId = "FLT_PICKLOC",
                    AreaId = "TL",
                    AisleId = "TL",
                    SectionId = "TL",
                    LevelId = "UL",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
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
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/products.json");
            var jsonData = File.ReadAllText(path);

            var products = JsonConvert.DeserializeObject<List<Product>>(jsonData);
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();

        if (!context.ProductAssocs.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/product_assocs.json");
            var jsonData = File.ReadAllText(path);

            var productAssocs = JsonConvert.DeserializeObject<List<ProductAssoc>>(jsonData);
            await context.ProductAssocs.AddRangeAsync(productAssocs);
            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();


        // select all products and loop
        var productsList = context.Products.ToList();
        var rnd = new Random();
        foreach (var product in productsList)
        {
            if (!context.ProductCategoryMembers.Any())
            {
                // for each product create a product category member
                var productCategoryMember = new ProductCategoryMember
                {
                    ProductId = product.ProductId,
                    ProductCategoryId = product.PrimaryProductCategoryId,
                    FromDate = nowDateTime.AddMonths(-2),
                    LastUpdatedStamp = nowDateTime.AddMonths(-2),
                    CreatedStamp = nowDateTime.AddMonths(-2)
                };
                context.ProductCategoryMembers.Add(productCategoryMember);
            }

            if (!context.ProductPrices.Any())
            {
                // for each product create a product price of type "LIST_PRICE"
                // with random price between 20 and 100 and neglect tax and priceWithTax
                var Price = rnd.Next(20, 100);

                var productListPrice = new ProductPrice
                {
                    ProductId = product.ProductId,
                    ProductPriceTypeId = "LIST_PRICE",
                    CurrencyUomId = "EGP",
                    FromDate = nowDateTime.AddMonths(-2),
                    Price = Price,
                    TaxAmount = 0,
                    CreatedStamp = nowDateTime.AddMonths(-2),
                    LastUpdatedStamp = nowDateTime.AddMonths(-2)
                };
                context.ProductPrices.Add(productListPrice);

                var productDefaultPrice = new ProductPrice
                {
                    ProductId = product.ProductId,
                    ProductPriceTypeId = "DEFAULT_PRICE",
                    CurrencyUomId = "EGP",
                    FromDate = nowDateTime.AddMonths(-2),
                    Price = Price,
                    TaxAmount = 0,
                    CreatedStamp = nowDateTime.AddMonths(-2),
                    LastUpdatedStamp = nowDateTime.AddMonths(-2)
                };
                context.ProductPrices.Add(productDefaultPrice);
            }
        }

        await context.SaveChangesAsync();

        // select all productPrices and loop
        var productPricesList = context.ProductPrices.ToList();
        foreach (var productPrice in productPricesList)
        {
            Console.WriteLine(productPrice.ProductId);
            Console.WriteLine(productPrice.ProductPriceTypeId);
            Console.WriteLine(productPrice.Price);
        }


        // Using the Faker library to generate random data, write a for loop to generate 100 products entity records
        // where the PrimaryProductCategoryId is in sparePartCategories and product type is "FINISHED_GOOD"
        // and the product name is unique and is compatible with primary product category
        // using tables Products and ProductCategories and ProductCategoryMembers only
        // and the product name is unique
        /*var rnd = new Random();

        var r = rnd.Next(sparePartCategories.Count);
        Randomizer.Seed = new Random();
        */


        // Products

        /*if (!context.Products.Any())
        {
            var randomForServiceLife = new Random();

            int randomForServiceLifeDays(int min, int max)
            {
                return randomForServiceLife.Next(min, max);
            }

            for (var i = 0; i < 100; i++)
            {
                var testProduct = new Faker<Product>()
                    .RuleFor(o => o.ProductId, f => Guid.NewGuid().ToString())
                    .RuleFor(o => o.PrimaryProductCategoryId, f => f.PickRandom(sparePartCategories).ProductCategoryId)
                    .RuleFor(o => o.ProductTypeId, f => "FINISHED_GOOD")
                    .RuleFor(o => o.IntroductionDate, f => nowDateTime.AddMonths(-2))
                    .RuleFor(o => o.ServiceLifeDays, f => randomForServiceLifeDays(90, 360))
                    .RuleFor(o => o.ServiceLifeMileage, f => randomForServiceLifeDays(2000, 5000))
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime.AddMonths(-2))
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime.AddMonths(-2))
                    .RuleFor(o => o.GoodIdentifications, f => new List<GoodIdentification>
                    {
                        new()
                        {
                            GoodIdentificationTypeId = "MANUFACTURER_ID_NO",
                            IdValue = f.Random.Replace("???-###-???"),
                            LastUpdatedStamp = nowDateTime.AddMonths(-2),
                            CreatedStamp = nowDateTime.AddMonths(-2)
                        },
                        new()
                        {
                            GoodIdentificationTypeId = "SKU",
                            IdValue = f.Random.ReplaceNumbers("###-??-###"),
                            LastUpdatedStamp = nowDateTime.AddMonths(-2),
                            CreatedStamp = nowDateTime.AddMonths(-2)
                        }
                    });

                var product = testProduct.Generate();
                product.ProductName = product.PrimaryProductCategoryId + " " + i;

                // Create separate instances of GoodIdentificationType for each GoodIdentification
                foreach (var goodIdentification in product.GoodIdentifications)
                    goodIdentification.GoodIdentificationType = context.GoodIdentificationTypes
                        .FirstOrDefault(t => t.GoodIdentificationTypeId == goodIdentification.GoodIdentificationTypeId);

                context.Products.Add(product);


                // for each product create a product category member
                var productCategoryMember = new ProductCategoryMember
                {
                    ProductId = product.ProductId,
                    ProductCategoryId = product.PrimaryProductCategoryId,
                    FromDate = nowDateTime.AddMonths(-2),
                    LastUpdatedStamp = nowDateTime.AddMonths(-2),
                    CreatedStamp = nowDateTime.AddMonths(-2)
                };
                context.ProductCategoryMembers.Add(productCategoryMember);


                // for each product create a product price of type "LIST_PRICE"
                // with random price between 20 and 100 and neglect tax and priceWithTax
                var Price = rnd.Next(20, 100);

                var productListPrice = new ProductPrice
                {
                    ProductId = product.ProductId,
                    ProductPriceTypeId = "LIST_PRICE",
                    CurrencyUomId = "EGP",
                    FromDate = nowDateTime.AddMonths(-2),
                    Price = Price,
                    TaxAmount = 0,
                    CreatedStamp = nowDateTime.AddMonths(-2),
                    LastUpdatedStamp = nowDateTime.AddMonths(-2)
                };
                context.ProductPrices.Add(productListPrice);

                var productDefaultPrice = new ProductPrice
                {
                    ProductId = product.ProductId,
                    ProductPriceTypeId = "DEFAULT_PRICE",
                    CurrencyUomId = "EGP",
                    FromDate = nowDateTime.AddMonths(-2),
                    Price = Price,
                    TaxAmount = 0,
                    CreatedStamp = nowDateTime.AddMonths(-2),
                    LastUpdatedStamp = nowDateTime.AddMonths(-2)
                };
                context.ProductPrices.Add(productDefaultPrice);
            }


            //create a temporary empty list of products
            var carServiceProducts = new List<Product>();

            foreach (var vsc in vehicleServiceCategories)
            {
                var testProduct = new Faker<Product>()
                    .RuleFor(o => o.ProductId, f => Guid.NewGuid().ToString())
                    .RuleFor(o => o.PrimaryProductCategoryId, vsc.ProductCategoryId)
                    .RuleFor(o => o.ProductTypeId, f => "SERVICE_PRODUCT")
                    .RuleFor(o => o.ProductName, vsc.Description)
                    .RuleFor(o => o.ServiceLifeDays, f => randomForServiceLifeDays(90, 360))
                    .RuleFor(o => o.ServiceLifeMileage, f => randomForServiceLifeDays(2000, 5000))
                    .RuleFor(o => o.IntroductionDate, f => nowDateTime.AddMonths(-2))
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime.AddMonths(-2))
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime.AddMonths(-2));

                var product = testProduct.Generate();
                context.Products.Add(product);
                carServiceProducts.Add(product);

                // for each product create a product category member
                var productCategoryMember = new ProductCategoryMember
                {
                    ProductId = product.ProductId,
                    ProductCategoryId = product.PrimaryProductCategoryId,
                    FromDate = nowDateTime.AddMonths(-2),
                    LastUpdatedStamp = nowDateTime.AddMonths(-2),
                    CreatedStamp = nowDateTime.AddMonths(-2)
                };
                context.ProductCategoryMembers.Add(productCategoryMember);
            }

            // create marketing packages
            var testProductMarketingPackage_1 = new Faker<Product>()
                .RuleFor(o => o.ProductId, f => Guid.NewGuid().ToString())
                .RuleFor(o => o.PrimaryProductCategoryId, f => "VEHICLE_MARKETING_PACKAGE")
                .RuleFor(o => o.ProductTypeId, f => "MARKETING_PKG")
                .RuleFor(o => o.IntroductionDate, f => nowDateTime.AddMonths(-2))
                .RuleFor(o => o.CreatedStamp, f => nowDateTime.AddMonths(-2))
                .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime.AddMonths(-2));


            var productMarketingPackage_1 = testProductMarketingPackage_1.Generate();
            productMarketingPackage_1.ProductName = "40k Service Package";


            context.Products.Add(productMarketingPackage_1);


            ////
            var vehicleMakes = context.ProductCategories.Where(x => x.PrimaryParentCategoryId == "VEHICLE_MAKE")
                .ToList();
            var random = new Random();

            foreach (var product in carServiceProducts)
            foreach (var vehicleMake in vehicleMakes)
            {
                // if vehicleMake is not CHEVROLET then skip
                if (vehicleMake.ProductCategoryId == "CHEVROLET") continue;
                var vehicleModels = context.ProductCategories
                    .Where(x => x.PrimaryParentCategoryId == vehicleMake.ProductCategoryId).ToList();

                foreach (var vehicleModel in vehicleModels)
                {
                    // Generate a random number between 30 and 90
                    var randomStandardTime = random.Next(30, 91);
                    var serviceSpecificationId = Guid.NewGuid().ToString();

                    // Create a new ServiceSpecification entry
                    var serviceSpecification = new ServiceSpecification
                    {
                        ServiceSpecificationId = serviceSpecificationId,
                        ProductId = product.ProductId,
                        MakeId = vehicleMake.ProductCategoryId,
                        ModelId = vehicleModel.ProductCategoryId,
                        StandardTimeInMinutes = randomStandardTime,
                        FromDate = nowDateTime,
                        CreatedStamp = nowDateTime.AddMonths(-2),
                        LastUpdatedStamp = nowDateTime.AddMonths(-2)
                    };

                    // Add serviceSpecification to the context
                    context.ServiceSpecifications.Add(serviceSpecification);
                }
            }

            await context.SaveChangesAsync();
        }*/

        /*if (!context.ProductAssocs.Any())
        {
            var productMarketingPackage_1 =
                context.Products.FirstOrDefault(x => x.ProductName == "40k Service Package");
            var importantSpareParts = new List<string> { "FUEL_PUMP", "RADIATOR", "GAUGE" };
            var importantServices = new List<string>
                { "Oil Change", "Tire Rotation and Balancing", "AC/Heater Repair or Replacement" };

            var selectedSpareParts = context.Products
                .Where(p => importantSpareParts.Contains(p.PrimaryProductCategoryId))
                .OrderByDescending(p => p.PrimaryProductCategoryId)
                .Take(3)
                .ToList();

            // Select the most important services
            var selectedServices = context.Products
                .Where(p => importantServices.Contains(p.ProductName))
                .OrderByDescending(p => p.ProductName)
                .Take(3)
                .ToList();

            // Add the selected spare parts to the package in table ProductAssoc
            foreach (var sparePart in selectedSpareParts)
            {
                var productAssoc = new ProductAssoc
                {
                    ProductAssocTypeId = "PRODUCT_COMPONENT",
                    ProductId = productMarketingPackage_1.ProductId,
                    ProductIdTo = sparePart.ProductId,
                    Quantity = 1,
                    FromDate = nowDateTime.AddMonths(-2),
                    CreatedStamp = nowDateTime.AddMonths(-2),
                    LastUpdatedStamp = nowDateTime.AddMonths(-2)
                };
                context.ProductAssocs.Add(productAssoc);
                context.SaveChanges();
            }

            // Add the selected services to the package in table ProductAssoc
            foreach (var service in selectedServices)
            {
                var productAssoc = new ProductAssoc
                {
                    ProductAssocTypeId = "PRODUCT_COMPONENT",
                    ProductId = productMarketingPackage_1.ProductId,
                    ProductIdTo = service.ProductId,
                    Quantity = 1,
                    FromDate = nowDateTime.AddMonths(-2),
                    CreatedStamp = nowDateTime.AddMonths(-2),
                    LastUpdatedStamp = nowDateTime.AddMonths(-2)
                };
                context.ProductAssocs.Add(productAssoc);
            }

            await context.SaveChangesAsync();
        }*/

        // Inventory Item Type
        if (!context.InventoryItemTypes.Any())
        {
            var inventoryItemTypes = new List<InventoryItemType>
            {
                new()
                {
                    InventoryItemTypeId = "NON_SERIAL_INV_ITEM",
                    Description = "Non-Serialized",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    InventoryItemTypeId = "SERIALIZED_INV_ITEM",
                    Description = "Serialized",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.InventoryItemTypes.AddRangeAsync(inventoryItemTypes);
            await context.SaveChangesAsync();
        }


        // select all product ids
        var productIds = context.Products.Where(x => x.ProductTypeId == "RAW_MATERIAL").Select(s => s.ProductId)
            .ToList();


        if (!context.ProductFacilities.Any())
        {
            // loop in productIds
            foreach (var productId in productIds)
            {
                // for each product create productFacilities
                var productFacility = new ProductFacility
                {
                    ProductId = productId,
                    FacilityId = guid6,
                    MinimumStock = 5,
                    ReorderQuantity = 5,
                    LastInventoryCount = 200,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                context.ProductFacilities.Add(productFacility);
            }

            await context.SaveChangesAsync();
        }


        // Customer request type
        if (!context.CustRequestTypes.Any())
        {
            var custRequestTypes = new List<CustRequestType>
            {
                new()
                {
                    CustRequestTypeId = "RF_QUOTE",
                    Description = "Request For Quote",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    CustRequestTypeId = "RF_PUR_QUOTE",
                    Description = "Request For Purchase Quote",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.CustRequestTypes.AddRangeAsync(custRequestTypes);
            await context.SaveChangesAsync();
        }


        // QUOTE type
        if (!context.QuoteTypes.Any())
        {
            var quoteTypes = new List<QuoteType>
            {
                new()
                {
                    QuoteTypeId = "PRODUCT_QUOTE",
                    Description = "Product",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    QuoteTypeId = "PURCHASE_QUOTE",
                    Description = "Product Purchase",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    QuoteTypeId = "JOB_QUOTE",
                    Description = "JOB SERVICE QUOTE",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    QuoteTypeId = "PROPOSAL",
                    Description = "Proposal",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.QuoteTypes.AddRangeAsync(quoteTypes);
            await context.SaveChangesAsync();
        }

        // Order type
        if (!context.OrderTypes.Any())
        {
            var orderTypes = new List<OrderType>
            {
                new()
                {
                    OrderTypeId = "PURCHASE_ORDER",
                    Description = "Purchase",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    OrderTypeId = "SALES_ORDER",
                    Description = "Sales",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.OrderTypes.AddRangeAsync(orderTypes);
            await context.SaveChangesAsync();
        }

        // Order Item type
        if (!context.OrderItemTypes.Any())
        {
            var orderItemTypes = new List<OrderItemType>
            {
                new()
                {
                    OrderItemTypeId = "PRODUCT_ORDER_ITEM",
                    Description = "Product Item",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
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


        var paymentSequenceRecord = await context.SequenceValueItems
            .Where(x => x.SeqName == "Payment").SingleOrDefaultAsync();

        if (!context.Parties.Any())
        {
            var companyId = "Company";
            var company = new Party
            {
                PartyId = companyId,
                PartyTypeId = "PARTY_GROUP",
                MainRole = "_NA_",
                StatusId = "PARTY_ENABLED",
                Description = "Company",
                CreatedStamp = nowDateTime,
                LastUpdatedStamp = nowDateTime
            };
            await context.Parties.AddAsync(company);

            var egyptianTaxAuthority = new Party
            {
                PartyId = "EgyptTaxAuth",
                PartyTypeId = "PARTY_GROUP",
                MainRole = "_NA_",
                StatusId = "PARTY_ENABLED",
                Description = "Egyptian Tax Authority",
                CreatedStamp = nowDateTime,
                LastUpdatedStamp = nowDateTime
            };
            await context.Parties.AddAsync(egyptianTaxAuthority);

            var _na_party = new Party
            {
                PartyId = "_NA_",
                PartyTypeId = "PARTY_GROUP",
                MainRole = "_NA_",
                StatusId = "PARTY_ENABLED",
                Description = "Not Available",
                CreatedStamp = nowDateTime,
                LastUpdatedStamp = nowDateTime
            };
            await context.Parties.AddAsync(_na_party);

            //loop in facilities and assign 'Company' as the owner of each facility
            var facilities = context.Facilities.ToList();
            foreach (var facility in facilities) facility.OwnerPartyId = companyId;


            var partyRoles = new List<PartyRole>
            {
                new()
                {
                    PartyId = companyId,
                    RoleTypeId = "_NA_",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    PartyId = companyId,
                    RoleTypeId = "BILL_FROM_VENDOR",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    PartyId = companyId,
                    RoleTypeId = "BILL_TO_CUSTOMER",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                },
                new()
                {
                    PartyId = companyId,
                    RoleTypeId = "INTERNAL_ORGANIZATIO",
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                }
            };
            await context.PartyRoles.AddRangeAsync(partyRoles);


            for (var i = 0; i < 50; i++)
            {
                var partyId = Guid.NewGuid().ToString();
                var firstName = new Faker().Name.FirstName();
                var lastName = new Faker().Name.LastName();
                var companyName = new Faker().Company.CompanyName();
                var fakeParty = new Faker<Party>()
                    .RuleFor(o => o.PartyId, f => partyId)
                    .RuleFor(o => o.PartyTypeId, i % 5 != 0 ? "PERSON" : "PARTY_GROUP")
                    .RuleFor(o => o.MainRole, i % 5 != 0 ? "CUSTOMER" : "SUPPLIER")
                    .RuleFor(o => o.StatusId, "PARTY_ENABLED")
                    .RuleFor(o => o.Description, i % 5 != 0 ? firstName + ' ' + lastName : companyName)
                    .RuleFor(o => o.CreatedDate, f => nowDateTime)
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.Parties.AddAsync(fakeParty);

                var fakePartyRole = new Faker<PartyRole>()
                    .RuleFor(o => o.PartyId, f => partyId)
                    .RuleFor(o => o.RoleTypeId, i % 5 != 0 ? "CUSTOMER" : "SUPPLIER")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.PartyRoles.AddAsync(fakePartyRole);

                var fakePartyRolePlacingCustomer = new Faker<PartyRole>()
                    .RuleFor(o => o.PartyId, f => partyId)
                    .RuleFor(o => o.RoleTypeId, i % 5 != 0 ? "PLACING_CUSTOMER" : "SHIP_FROM_VENDOR")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.PartyRoles.AddAsync(fakePartyRolePlacingCustomer);

                var fakePartyRoleBilToCustomer = new Faker<PartyRole>()
                    .RuleFor(o => o.PartyId, f => partyId)
                    .RuleFor(o => o.RoleTypeId, i % 5 != 0 ? "BILL_TO_CUSTOMER" : "BILL_FROM_VENDOR")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);
                await context.PartyRoles.AddAsync(fakePartyRoleBilToCustomer);

                if (i % 5 == 0)
                {
                    var fakePartyRoleSUPPLIER_AGENT = new Faker<PartyRole>()
                        .RuleFor(o => o.PartyId, f => partyId)
                        .RuleFor(o => o.RoleTypeId, "SUPPLIER_AGENT")
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);
                    await context.PartyRoles.AddAsync(fakePartyRoleSUPPLIER_AGENT);
                }

                if (i % 5 != 0)
                {
                    decimal randomAccountLimit = new Random().Next(100000, 250001);

                    // create a BillingAccount for each customer
                    var billingAccount = new BillingAccount
                    {
                        BillingAccountId = Guid.NewGuid().ToString(),
                        AccountLimit = randomAccountLimit,
                        AccountCurrencyUomId = "EGP",
                        FromDate = nowDateTime,
                        CreatedStamp = nowDateTime,
                        LastUpdatedStamp = nowDateTime
                    };
                    await context.BillingAccounts.AddAsync(billingAccount);

                    // create a BillingAccountRole for each customer
                    var billingAccountRole = new BillingAccountRole
                    {
                        BillingAccountId = billingAccount.BillingAccountId,
                        PartyId = partyId,
                        RoleTypeId = "BILL_TO_CUSTOMER",
                        FromDate = nowDateTime,
                        CreatedStamp = nowDateTime,
                        LastUpdatedStamp = nowDateTime
                    };
                    await context.BillingAccountRoles.AddAsync(billingAccountRole);
                }

                if (i % 5 != 0)
                {
                    var fakePerson = new Faker<Person>()
                        .RuleFor(o => o.PartyId, f => partyId)
                        .RuleFor(o => o.FirstName, f => firstName + ' ' + lastName)
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                    await context.Persons.AddAsync(fakePerson);


                    var fakeOrder = new Faker<OrderHeader>()
                        .RuleFor(o => o.StatusId, "ORDER_CREATED")
                        .RuleFor(o => o.SalesChannelEnumId, "PHONE_SALES_CHANNEL")
                        .RuleFor(o => o.OrderTypeId, "SALES_ORDER")
                        .RuleFor(o => o.OrderId, "SORDER-" + i)
                        .RuleFor(o => o.OrderDate, f => nowDateTime)
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                    await context.OrderHeaders.AddAsync(fakeOrder);

                    var orderPaymentPreferenceId = Guid.NewGuid().ToString();
                    var fakeOrderPaymentPreferences = new Faker<OrderPaymentPreference>()
                        .RuleFor(o => o.StatusId, "PAYMENT_NOT_RECEIVED")
                        .RuleFor(o => o.OrderPaymentPreferenceId, orderPaymentPreferenceId)
                        .RuleFor(o => o.PaymentMethodTypeId, "CASH")
                        .RuleFor(o => o.OrderId, "SORDER-" + i)
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                    await context.OrderPaymentPreferences.AddAsync(fakeOrderPaymentPreferences);

                    var fakeOrderRole = new Faker<OrderRole>()
                        .RuleFor(o => o.PartyId, partyId)
                        .RuleFor(o => o.RoleTypeId, "PLACING_CUSTOMER")
                        .RuleFor(o => o.OrderId, "SORDER-" + i)
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                    await context.OrderRoles.AddAsync(fakeOrderRole);

                    //select default_price from product_prices where product_id = productIds[0]
                    var productPriceForProductId0 = await context.ProductPrices
                        .Where(x => x.ProductId == productIds[0] && x.ProductPriceTypeId == "DEFAULT_PRICE")
                        .SingleOrDefaultAsync();

                    //select default_price from product_prices where product_id = productIds[1]
                    var productPriceForProductId1 = await context.ProductPrices
                        .Where(x => x.ProductId == productIds[1] && x.ProductPriceTypeId == "DEFAULT_PRICE")
                        .SingleOrDefaultAsync();

                    var fakeOrderItem1 = new Faker<OrderItem>()
                        .RuleFor(o => o.OrderItemSeqId, "01")
                        .RuleFor(o => o.ProductId, productIds[1])
                        .RuleFor(o => o.OrderItemTypeId, "PRODUCT_ORDER_ITEM")
                        .RuleFor(o => o.OrderId, "SORDER-" + i)
                        .RuleFor(o => o.Quantity, 2)
                        .RuleFor(o => o.UnitPrice, productPriceForProductId1.Price)
                        .RuleFor(o => o.UnitListPrice, productPriceForProductId1.Price)
                        .RuleFor(o => o.StatusId, "ITEM_CREATED")
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                    var fakeOrderItem2 = new Faker<OrderItem>()
                        .RuleFor(o => o.OrderItemSeqId, "02")
                        .RuleFor(o => o.ProductId, productIds[0])
                        .RuleFor(o => o.OrderItemTypeId, "PRODUCT_ORDER_ITEM")
                        .RuleFor(o => o.OrderId, "SORDER-" + i)
                        .RuleFor(o => o.Quantity, 1)
                        .RuleFor(o => o.UnitPrice, productPriceForProductId0.Price)
                        .RuleFor(o => o.UnitListPrice, productPriceForProductId0.Price)
                        .RuleFor(o => o.StatusId, "ITEM_CREATED")
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                    await context.OrderItems.AddAsync(fakeOrderItem1);
                    await context.OrderItems.AddAsync(fakeOrderItem2);

                    var fakeReturnHeaderId = i + 50.ToString();
                    var fakeReturnHeader = new Faker<ReturnHeader>()
                        .RuleFor(o => o.ReturnId, fakeReturnHeaderId)
                        .RuleFor(o => o.ReturnHeaderTypeId, "CUSTOMER_RETURN")
                        .RuleFor(o => o.StatusId, "RETURN_REQUESTED")
                        .RuleFor(o => o.FromPartyId, partyId)
                        .RuleFor(o => o.ToPartyId, "Company")
                        .RuleFor(o => o.DestinationFacilityId, f => guid6)
                        .RuleFor(o => o.EntryDate, f => nowDateTime)
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);
                    await context.ReturnHeaders.AddAsync(fakeReturnHeader);

                    var fakeReturnItem1 = new Faker<ReturnItem>()
                        .RuleFor(o => o.ReturnId, fakeReturnHeaderId)
                        .RuleFor(o => o.ReturnItemSeqId, "01")
                        .RuleFor(o => o.OrderId, "SORDER-" + i)
                        .RuleFor(o => o.OrderItemSeqId, "01")
                        .RuleFor(o => o.ReturnTypeId, "RTN_REFUND")
                        .RuleFor(o => o.ReturnItemTypeId, "RET_FPROD_ITEM")
                        .RuleFor(o => o.StatusId, "RETURN_REQUESTED")
                        .RuleFor(o => o.ProductId, productIds[1])
                        .RuleFor(o => o.ReturnQuantity, 1)
                        .RuleFor(o => o.ReturnPrice, 11)
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);
                    await context.ReturnItems.AddAsync(fakeReturnItem1);


                    var fakeOrderAdjustment1 = new Faker<OrderAdjustment>()
                        .RuleFor(o => o.OrderAdjustmentId, Guid.NewGuid().ToString())
                        .RuleFor(o => o.OrderId, "SORDER-" + i)
                        .RuleFor(o => o.OrderAdjustmentTypeId, "DISCOUNT_ADJUSTMENT")
                        .RuleFor(o => o.OrderItemSeqId, "_NA_")
                        .RuleFor(o => o.IsManual, "Y")
                        .RuleFor(o => o.Amount, -5)
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                    var fakeOrderAdjustment2 = new Faker<OrderAdjustment>()
                        .RuleFor(o => o.OrderAdjustmentId, Guid.NewGuid().ToString())
                        .RuleFor(o => o.OrderId, "SORDER-" + i)
                        .RuleFor(o => o.OrderAdjustmentTypeId, "DISCOUNT_ADJUSTMENT")
                        .RuleFor(o => o.OrderItemSeqId, "01")
                        .RuleFor(o => o.CorrespondingProductId, productIds[1])
                        .RuleFor(o => o.IsManual, "Y")
                        .RuleFor(o => o.Amount, -5)
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                    var fakeOrderAdjustment3 = new Faker<OrderAdjustment>()
                        .RuleFor(o => o.OrderAdjustmentId, Guid.NewGuid().ToString())
                        .RuleFor(o => o.OrderId, "SORDER-" + i)
                        .RuleFor(o => o.OrderAdjustmentTypeId, "DISCOUNT_ADJUSTMENT")
                        .RuleFor(o => o.OrderItemSeqId, "02")
                        .RuleFor(o => o.CorrespondingProductId, productIds[0])
                        .RuleFor(o => o.IsManual, "Y")
                        .RuleFor(o => o.Amount, -3)
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);


                    await context.OrderAdjustments.AddAsync(fakeOrderAdjustment1);
                    await context.OrderAdjustments.AddAsync(fakeOrderAdjustment2);
                    await context.OrderAdjustments.AddAsync(fakeOrderAdjustment3);

                    var invoices = new List<Invoice>
                    {
                        new()
                        {
                            InvoiceId = "INV-" + i,
                            InvoiceTypeId = "SALES_INVOICE",
                            PartyIdFrom = "Company",
                            InvoiceDate = nowDateTime,
                            StatusId = "INVOICE_IN_PROCESS",
                            PartyId = partyId,
                            CurrencyUomId = "EGP",
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        }
                    };
                    await context.Invoices.AddRangeAsync(invoices);
                    Console.WriteLine(partyId);

                    var invoiceRoles = new List<InvoiceRole>
                    {
                        new()
                        {
                            InvoiceId = "INV-" + i,
                            PartyId = partyId,
                            RoleTypeId = "PLACING_CUSTOMER",
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        },
                        new()
                        {
                            InvoiceId = "INV-" + i,
                            PartyId = partyId,
                            RoleTypeId = "BILL_TO_CUSTOMER",
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        }
                    };
                    await context.InvoiceRoles.AddRangeAsync(invoiceRoles);

                    var invoiceItems = new List<InvoiceItem>
                    {
                        new()
                        {
                            InvoiceId = "INV-" + i,
                            InvoiceItemSeqId = "01",
                            InvoiceItemTypeId = "INV_FPROD_ITEM",
                            ProductId = productIds[1],
                            Quantity = 2,
                            Amount = 34,
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        }
                    };
                    await context.InvoiceItems.AddRangeAsync(invoiceItems);

                    var paymentId = paymentSequenceRecord.SeqId - 100 + i;
                    var payment = new List<Payment>
                    {
                        new()
                        {
                            PaymentId = paymentId.ToString(),
                            PaymentTypeId = "CUSTOMER_PAYMENT",
                            PaymentMethodTypeId = "EXT_OFFLINE",
                            PaymentPreferenceId = orderPaymentPreferenceId,
                            StatusId = "PMNT_NOT_PAID",
                            PartyIdFrom = partyId,
                            PartyIdTo = "Company",
                            CurrencyUomId = "EGP",
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        }
                    };
                    await context.Payments.AddRangeAsync(payment);

                    var paymentApplications = new List<PaymentApplication>
                    {
                        new()
                        {
                            PaymentApplicationId = Guid.NewGuid().ToString(),
                            PaymentId = paymentId.ToString(),
                            InvoiceId = "INV-" + i,
                            InvoiceItemSeqId = "01",
                            AmountApplied = 34,
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        }
                    };
                    await context.PaymentApplications.AddRangeAsync(paymentApplications);
                }
                else
                {
                    var fakePartyGroup = new Faker<PartyGroup>()
                        .RuleFor(o => o.PartyId, f => partyId)
                        .RuleFor(o => o.GroupName, companyName)
                        .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                        .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                    await context.PartyGroups.AddAsync(fakePartyGroup);

                    //loop through products and add them to the supplier
                    for (var j = 0; j < productIds.Count - 1; j++)
                    {
                        var fakeProductSupplier = new Faker<SupplierProduct>()
                            .RuleFor(o => o.PartyId, f => partyId)
                            .RuleFor(o => o.ProductId, productIds[j])
                            .RuleFor(o => o.LastPrice, 20)
                            .RuleFor(o => o.SupplierPrefOrderId, "10_MAIN_SUPPL")
                            .RuleFor(o => o.CurrencyUomId, "EGP")
                            .RuleFor(o => o.MinimumOrderQuantity, f => 2)
                            .RuleFor(o => o.AvailableFromDate, f => nowDateTime)
                            .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                            .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                        await context.SupplierProducts.AddAsync(fakeProductSupplier);
                    }
                }


                var fakePartyStatus = new Faker<PartyStatus>()
                    .RuleFor(o => o.PartyId, f => partyId)
                    .RuleFor(o => o.StatusDate, f => nowDateTime)
                    .RuleFor(o => o.StatusId, f => "PARTY_ENABLED")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.PartyStatuses.AddAsync(fakePartyStatus);

                var contactMechId1 = Guid.NewGuid().ToString();
                var fakeContactMechTelcom = new Faker<ContactMech>()
                    .RuleFor(o => o.ContactMechId, f => contactMechId1)
                    .RuleFor(o => o.ContactMechTypeId, "TELECOM_NUMBER")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.ContactMeches.AddAsync(fakeContactMechTelcom);

                var contactMechId2 = Guid.NewGuid().ToString();
                var fakeContactMechEmail = new Faker<ContactMech>()
                    .RuleFor(o => o.ContactMechId, f => contactMechId2)
                    .RuleFor(o => o.ContactMechTypeId, "EMAIL_ADDRESS")
                    .RuleFor(o => o.InfoString, f => f.Person.Email)
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.ContactMeches.AddAsync(fakeContactMechEmail);

                var contactMechId3 = Guid.NewGuid().ToString();
                var fakeContactMechPostal = new Faker<ContactMech>()
                    .RuleFor(o => o.ContactMechId, f => contactMechId3)
                    .RuleFor(o => o.ContactMechTypeId, "POSTAL_ADDRESS")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.ContactMeches.AddAsync(fakeContactMechPostal);

                var fakeTelcomNo = new Faker<TelecomNumber>()
                    .RuleFor(o => o.ContactMechId, f => contactMechId1)
                    .RuleFor(o => o.ContactNumber, f => "011" + f.Random.Replace("########"))
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.TelecomNumbers.AddAsync(fakeTelcomNo);

                var fakeTelPostal = new Faker<PostalAddress>()
                    .RuleFor(o => o.ContactMechId, f => contactMechId2)
                    .RuleFor(o => o.ToName, f => firstName + ' ' + lastName)
                    .RuleFor(o => o.Address1, f => f.Address.StreetName())
                    .RuleFor(o => o.Address2, f => f.Address.BuildingNumber())
                    .RuleFor(o => o.CountryGeoId, f => "EGY")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.PostalAddresses.AddAsync(fakeTelPostal);

                var fakePartyContactMech1 = new Faker<PartyContactMech>()
                    .RuleFor(o => o.ContactMechId, f => contactMechId1)
                    .RuleFor(o => o.PartyId, partyId)
                    .RuleFor(o => o.RoleTypeId, i % 5 != 0 ? "CUSTOMER" : "SUPPLIER")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.FromDate, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.PartyContactMeches.AddAsync(fakePartyContactMech1);

                var fakePartyContactMech2 = new Faker<PartyContactMech>()
                    .RuleFor(o => o.ContactMechId, f => contactMechId2)
                    .RuleFor(o => o.PartyId, partyId)
                    .RuleFor(o => o.RoleTypeId, i % 5 != 0 ? "CUSTOMER" : "SUPPLIER")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.FromDate, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.PartyContactMeches.AddAsync(fakePartyContactMech2);

                var fakePartyContactMech3 = new Faker<PartyContactMech>()
                    .RuleFor(o => o.ContactMechId, f => contactMechId3)
                    .RuleFor(o => o.PartyId, partyId)
                    .RuleFor(o => o.RoleTypeId, i % 5 != 0 ? "CUSTOMER" : "SUPPLIER")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.FromDate, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.PartyContactMeches.AddAsync(fakePartyContactMech3);

                var fakePartyContactMechPurpose1 = new Faker<PartyContactMechPurpose>()
                    .RuleFor(o => o.ContactMechId, f => contactMechId1)
                    .RuleFor(o => o.PartyId, partyId)
                    .RuleFor(o => o.ContactMechPurposeTypeId, "PRIMARY_PHONE")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.FromDate, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.PartyContactMechPurposes.AddAsync(fakePartyContactMechPurpose1);

                var fakePartyContactMechPurpose2 = new Faker<PartyContactMechPurpose>()
                    .RuleFor(o => o.ContactMechId, f => contactMechId2)
                    .RuleFor(o => o.PartyId, partyId)
                    .RuleFor(o => o.ContactMechPurposeTypeId, "PRIMARY_EMAIL")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.FromDate, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.PartyContactMechPurposes.AddAsync(fakePartyContactMechPurpose2);

                var fakePartyContactMechPurpose3 = new Faker<PartyContactMechPurpose>()
                    .RuleFor(o => o.ContactMechId, f => contactMechId3)
                    .RuleFor(o => o.PartyId, partyId)
                    .RuleFor(o => o.ContactMechPurposeTypeId, "GENERAL_LOCATION")
                    .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                    .RuleFor(o => o.FromDate, f => nowDateTime)
                    .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);

                await context.PartyContactMechPurposes.AddAsync(fakePartyContactMechPurpose3);
            }


            await context.SaveChangesAsync();
        }


        // loop in supplierProducts and select the productIds and suppliersIds and insert into inventoryItem
        var productSuppliers = await context.SupplierProducts
            .ToListAsync();


        foreach (var productId in productSuppliers)
            if (!context.InventoryItems.Any())
            {
                var inventoryItemNewSerialRecord = await context.SequenceValueItems
                    .Where(x => x.SeqName == "InventoryItem").SingleOrDefaultAsync();
                var newInventoryItemSerial = inventoryItemNewSerialRecord.SeqId + 1;

                var inventoryItem = new InventoryItem
                {
                    InventoryItemId = newInventoryItemSerial.ToString(),
                    InventoryItemTypeId = "NON_SERIAL_INV_ITEM",
                    OwnerPartyId = "Company",
                    CurrencyUomId = "EGP",
                    ProductId = productId.ProductId,
                    PartyId = productId.PartyId,
                    FacilityId = guid6,
                    BinNumber = "1",
                    LocationSeqId = "TLTLTLUL01",
                    QuantityOnHandTotal = 200,
                    AvailableToPromiseTotal = 200,
                    AccountingQuantityTotal = 200,
                    UnitCost = 28,
                    DatetimeReceived = nowDateTime,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                context.InventoryItems.Add(inventoryItem);

                var inventoryItemDetailNewSerialRecord = await context.SequenceValueItems
                    .Where(x => x.SeqName == "InventoryItemDetail").SingleOrDefaultAsync();
                var newInventoryItemDetailSerial = inventoryItemDetailNewSerialRecord.SeqId + 1;

                var inventoryItemDetail = new InventoryItemDetail
                {
                    InventoryItemId = newInventoryItemSerial.ToString(),
                    InventoryItemDetailSeqId = newInventoryItemDetailSerial.ToString(),
                    AvailableToPromiseDiff = 200,
                    AccountingQuantityDiff = 200,
                    QuantityOnHandDiff = 200,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                context.InventoryItemDetails.Add(inventoryItemDetail);

                // update inventoryItemNewSerialRecord
                inventoryItemNewSerialRecord.SeqId = newInventoryItemSerial;
                context.SequenceValueItems.Update(inventoryItemNewSerialRecord);
                // update inventoryItemDetailNewSerialRecord
                inventoryItemDetailNewSerialRecord.SeqId = newInventoryItemDetailSerial;
                context.SequenceValueItems.Update(inventoryItemDetailNewSerialRecord);
            }

        await context.SaveChangesAsync();


        // rejection reasons
        if (!context.RejectionReasons.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/rejection_reasons.json");
            var jsonData = File.ReadAllText(path);

            var rejectionReasons = JsonConvert.DeserializeObject<List<RejectionReason>>(jsonData);
            await context.RejectionReasons.AddRangeAsync(rejectionReasons);
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

        // Product Stores
        var guidProductStore = Guid.NewGuid().ToString();
        if (!context.ProductStores.Any())
        {
            var productStores = new List<ProductStore>
            {
                new()
                {
                    ProductStoreId = "9000",
                    StoreName = "Main Store",
                    InventoryFacilityId = guid6,
                    PayToPartyId = "Company",
                    DaysToCancelNonPay = 30,
                    ReserveOrderEnumId = "INVRO_FIFO_REC",
                    SOrderNumberPrefix = "SO",
                    POrderNumberPrefix = "PO",
                    DefaultCurrencyUomId = "EGP",
                    VatTaxAuthGeoId = "EGY",
                    VatTaxAuthPartyId = "EgyptTaxAuth",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
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


        // Add Vehicle Service Rates
        /*if (!context.ServiceRates.Any())
        {
            var vehicleMakes1 = context.ProductCategories.Where(x => x.PrimaryParentCategoryId == "VEHICLE_MAKE")
                .ToList();
            var random1 = new Random();
            foreach (var vehicleMake in vehicleMakes1)
            {
                // if vehicle make is not CHEVROLET, skip
                if (vehicleMake.ProductCategoryId == "CHEVROLET") continue;

                var vehicleModels = context.ProductCategories
                    .Where(x => x.PrimaryParentCategoryId == vehicleMake.ProductCategoryId).ToList();

                foreach (var vehicleModel in vehicleModels)
                {
                    // Generate a random number between 30 and 90
                    var randomHourRate = random1.Next(100, 300);
                    var serviceRateId = Guid.NewGuid().ToString();

                    var serviceRate = new ServiceRate
                    {
                        ServiceRateId = serviceRateId,
                        MakeId = vehicleMake.ProductCategoryId,
                        ModelId = vehicleModel.ProductCategoryId,
                        ProductStoreId = "9000",
                        Rate = randomHourRate,
                        FromDate = nowDateTime,
                        CreatedStamp = nowDateTime.AddMonths(-2),
                        LastUpdatedStamp = nowDateTime.AddMonths(-2)
                    };


                    context.ServiceRates.Add(serviceRate);
                }
            }

            await context.SaveChangesAsync();
        }
        */


        //Add Vehicles
        /*if (!context.Vehicles.Any())
        {
            // Create 20 vehicles for each model and make combination, with random years between 2000 and 2020
            // and random mileage between 1000 and 200000
            // and random ServiceDate which is the last service date between 1 and 30 days ago
            // and a random ServiceNextDate which is the next service date between 90 and 120 days after the last service date
            // and a random VehicleTypeId that can ve derived from ProductCategory table where primary_parent_category_id = VEHICLE_TYPE
            // and a random TransmissionTypeId that can ve derived from ProductCategory table where primary_parent_category_id = VEHICLE_TRANSMISSION_TYPE
            // and a random ExteriorColorId that can ve derived from ProductCategory table where primary_parent_category_id = VEHICLE_EXTERIOR_COLOR
            // and a random InteriorColorId that can ve derived from ProductCategory table where primary_parent_category_id = VEHICLE_INTERIOR_COLOR
            // and a random OwnerPartyId that can ve derived from Party table
            // and assign a value - using the Faker library - for ChassisNumber, Vin, PlateNumber - preferably unique using the egyption plate number format

            var vehicleMakes2 = context.ProductCategories.Where(x => x.PrimaryParentCategoryId == "VEHICLE_MAKE")
                .ToList();
            var random2 = new Random();
            foreach (var vehicleMake in vehicleMakes2)
            {
                var vehicleModels = context.ProductCategories
                    .Where(x => x.PrimaryParentCategoryId == vehicleMake.ProductCategoryId).ToList();

                foreach (var vehicleModel in vehicleModels)
                    for (var i = 0; i < 20; i++)
                    {
                        var randomYear = random2.Next(2000, 2020);
                        var randomMileage = random2.Next(1000, 200000);
                        var randomServiceDate = nowDateTime.AddDays(-random2.Next(1, 30));
                        var randomServiceNextDate = randomServiceDate.AddDays(random2.Next(90, 120));
                        var vehicleTypeId = context.ProductCategories
                            .Where(x => x.PrimaryParentCategoryId == "VEHICLE_TYPE")
                            .OrderBy(x => Guid.NewGuid()).FirstOrDefault()?.ProductCategoryId;
                        var transmissionTypeId = context.ProductCategories
                            .Where(x => x.PrimaryParentCategoryId == "VEHICLE_TRANSMISSION_TYPE")
                            .OrderBy(x => Guid.NewGuid()).FirstOrDefault()?.ProductCategoryId;
                        var exteriorColorId = context.ProductCategories
                            .Where(x => x.PrimaryParentCategoryId == "VEHICLE_EXTERIOR_COLOR")
                            .OrderBy(x => Guid.NewGuid()).FirstOrDefault()?.ProductCategoryId;
                        var interiorColorId = context.ProductCategories
                            .Where(x => x.PrimaryParentCategoryId == "VEHICLE_INTERIOR_COLOR")
                            .OrderBy(x => Guid.NewGuid()).FirstOrDefault()?.ProductCategoryId;
                        var fromPartyId = context.Parties
                            .Where(z => z.PartyTypeId == "PERSON" && z.PartyId != "Company")
                            .OrderBy(x => Guid.NewGuid()).FirstOrDefault()?.PartyId;

                        var faker = new Faker();
                        var vin = faker.Vehicle.Vin();

                        // Check if the vehicle make is CHEVROLET and modify the VIN accordingly
                        if (vehicleMake.ProductCategoryId == "CHEVROLET")
                            vin = "CHE" + vin.Substring(3);

                        string chassisNumber;

                        chassisNumber = vin.Substring(0, 9);

                        var vehicleId = Guid.NewGuid().ToString();

                        var testVehicle = new Faker<Vehicle>()
                            .RuleFor(o => o.VehicleId, vehicleId)
                            .RuleFor(o => o.ChassisNumber, f => chassisNumber)
                            .RuleFor(o => o.Vin, f => vin)
                            .RuleFor(o => o.PlateNumber, f => faker.Random.Replace("?? ####"))
                            .RuleFor(o => o.Year, f => randomYear)
                            .RuleFor(o => o.Mileage, f => randomMileage)
                            .RuleFor(o => o.ServiceDate, f => randomServiceDate)
                            .RuleFor(o => o.NextServiceDate, f => randomServiceNextDate)
                            .RuleFor(o => o.MakeId, f => vehicleMake.ProductCategoryId)
                            .RuleFor(o => o.ModelId, f => vehicleModel.ProductCategoryId)
                            .RuleFor(o => o.FromPartyId, f => fromPartyId)
                            .RuleFor(o => o.TransmissionTypeId, f => transmissionTypeId)
                            .RuleFor(o => o.ExteriorColorId, f => exteriorColorId)
                            .RuleFor(o => o.InteriorColorId, f => interiorColorId)
                            .RuleFor(o => o.VehicleTypeId, f => vehicleTypeId)
                            .RuleFor(o => o.CreatedStamp, f => nowDateTime)
                            .RuleFor(o => o.LastUpdatedStamp, f => nowDateTime);


                        var vehicle = testVehicle.Generate();
                        context.Vehicles.Add(vehicle);

                        var dataResourceId = Guid.NewGuid().ToString();
                        var contentId = Guid.NewGuid().ToString();

                        // add DataResource to hold vehicle file attachment
                        var dataResource = new DataResource
                        {
                            DataResourceId = dataResourceId,
                            DataResourceTypeId = "LOCAL_FILE",
                            DataResourceName = "mypicture.jpeg",
                            ObjectInfo = "https://nyc3.digitaloceanspaces.com/businessonefiles/mypicture.jpeg",
                            MimeTypeId = "image/gif",
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        };
                        context.DataResources.Add(dataResource);

                        // add Content to hold vehicle file attachment
                        var content = new Content
                        {
                            ContentId = contentId,
                            DataResourceId = dataResourceId,
                            ContentTypeId = "DOCUMENT",
                            ContentName = "mypicture.jpeg",
                            MimeTypeId = "image/gif",
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        };
                        context.Contents.Add(content);

                        // add VehicleContent to hold vehicle file attachment
                        var vehicleContent = new VehicleContent
                        {
                            VehicleId = vehicleId,
                            ContentId = contentId,
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        };
                        context.VehicleContents.Add(vehicleContent);

                        // add Annotation for damages in vehicle
                        var annotationId = Guid.NewGuid().ToString();
                        var annotation = new Annotation
                        {
                            AnnotationId = annotationId,
                            XCoordinate = faker.Random.Number(0, 100),
                            YCoordinate = faker.Random.Number(0, 100),
                            Note = faker.Lorem.Sentence(),
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        };
                        context.Annotations.Add(annotation);

                        var vehicleAnnotationId = Guid.NewGuid().ToString();
                        // add VehicleAnnotation to hold vehicle file attachment
                        var vehicleAnnotation = new VehicleAnnotation
                        {
                            VehicleId = vehicleId,
                            AnnotationId = annotationId,
                            VehicleAnnotationId = vehicleAnnotationId,
                            CreatedStamp = nowDateTime,
                            LastUpdatedStamp = nowDateTime
                        };
                        context.VehicleAnnotations.Add(vehicleAnnotation);
                    }
            }

            await context.SaveChangesAsync();
        }
        */

        /*if (!context.Quotes.Any())
        {
            var importantSpareParts = new List<string> { "FUEL_PUMP", "RADIATOR", "GAUGE" };
            var importantServices = new List<string>
                { "Oil Change", "Tire Rotation and Balancing", "AC/Heater Repair or Replacement" };

            var selectedSpareParts = context.Products
                .Where(p => importantSpareParts.Contains(p.PrimaryProductCategoryId))
                .OrderByDescending(p => p.PrimaryProductCategoryId)
                .Take(3)
                .ToList();

            // Select the most important services
            var selectedServices = context.Products
                .Where(p => importantServices.Contains(p.ProductName))
                .OrderByDescending(p => p.ProductName)
                .Take(3)
                .ToList();
            // select random 5 vehicles
            var vehicles = context.Vehicles.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
            // for each vehicle create a quote that has 3 qouteItems
            // then attach the quote to a VehicleJobQuote and for
            // quoteItems attach the selected spare parts and services
            foreach (var vehicle in vehicles)
            {
                var sequenceRecord = await context.SequenceValueItems
                    .Where(x => x.SeqName == "Quote").SingleOrDefaultAsync();

                var newSequence = sequenceRecord.SeqId + 1;
                sequenceRecord.SeqId = newSequence;
                var quote = new Quote
                {
                    QuoteId = newSequence.ToString(),
                    QuoteTypeId = "JOB_QUOTE",
                    StatusId = "QUO_CREATED",
                    VehicleId = vehicle.VehicleId,
                    PartyId = vehicle.FromPartyId,
                    CustomerRemarks = "Customer Remarks",
                    InternalRemarks = "Internal Remarks",
                    IssueDate = nowDateTime,
                    ValidFromDate = nowDateTime,
                    ValidThruDate = nowDateTime.AddDays(7),
                    GrandTotal = 27,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                context.Quotes.Add(quote);

                var index = 1;
                foreach (var sparePart in selectedSpareParts)
                {
                    var quoteItem = new QuoteItem
                    {
                        QuoteId = newSequence.ToString(),
                        QuoteItemSeqId = index++.ToString(),
                        ProductId = sparePart.ProductId,
                        Quantity = 1,
                        QuoteUnitListPrice = 23,
                        QuoteUnitPrice = 27,
                        CreatedStamp = nowDateTime,
                        LastUpdatedStamp = nowDateTime
                    };
                    context.QuoteItems.Add(quoteItem);
                }

                foreach (var service in selectedServices)
                {
                    var quoteItem = new QuoteItem
                    {
                        QuoteId = newSequence.ToString(),
                        QuoteItemSeqId = index++.ToString(),
                        ProductId = service.ProductId,
                        Quantity = 1,
                        QuoteUnitPrice = 43,
                        CreatedStamp = nowDateTime,
                        LastUpdatedStamp = nowDateTime
                    };
                    context.QuoteItems.Add(quoteItem);
                }
            }


            await context.SaveChangesAsync();
        }
        */


        // Product Stores Facility
        if (!context.ProductStoreFacilities.Any())
        {
            var productStoreFacilities = new List<ProductStoreFacility>
            {
                new()
                {
                    ProductStoreId = "9000",
                    FacilityId = guid6,
                    FromDate = nowDateTime,
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.ProductStoreFacilities.AddRangeAsync(productStoreFacilities);
            await context.SaveChangesAsync();
        }


        // tax authority categories
        if (!context.TaxAuthorityCategories.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/tax_authority_categories_Chocolate.json");
            var jsonData = File.ReadAllText(path);

            var taxAuthorityCategories = JsonConvert.DeserializeObject<List<TaxAuthorityCategory>>(jsonData);
            await context.TaxAuthorityCategories.AddRangeAsync(taxAuthorityCategories);
            await context.SaveChangesAsync();
        }

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
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/fixed_asset_types.json");
            var jsonData = File.ReadAllText(path);

            var fixedAssetTypes = JsonConvert.DeserializeObject<List<FixedAssetType>>(jsonData);
            await context.FixedAssetTypes.AddRangeAsync(fixedAssetTypes);
            await context.SaveChangesAsync();
        }

        // fixed assets
        if (!context.FixedAssets.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Json/fixed_assets.json");
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


        var categoryCounts = new Dictionary<string, int>();
        var productPromoIds = new List<string>();

        // Step 1 & 2: Populate the dictionary
        /*foreach (var product in context.Products)
            if (categoryCounts.ContainsKey(product.PrimaryProductCategoryId))
                categoryCounts[product.PrimaryProductCategoryId]++;
            else
                categoryCounts[product.PrimaryProductCategoryId] = 1;

        // Step 3 & 4: Gather the ProductIds
        foreach (var product in context.Products)
            if (categoryCounts[product.PrimaryProductCategoryId] >= 2)
                productPromoIds.Add(product.ProductId);

        // Print the results
        foreach (var id in productPromoIds) Console.WriteLine(id);
        */


        // product promo product
        if (!context.ProductPromoProducts.Any())
        {
            var productPromoProducts = new List<ProductPromoProduct>
            {
                new()
                {
                    ProductPromoId = "9015",
                    ProductPromoRuleId = "01",
                    ProductId = productPromoIds[1],
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
            var productPromoCategories = new List<ProductPromoCategory>
            {
                new()
                {
                    ProductPromoId = "9013",
                    ProductPromoRuleId = "01",
                    ProductCategoryId = "RAW_MATERIALS",
                    ProductPromoActionSeqId = "01",
                    ProductPromoCondSeqId = "_NA_",
                    AndGroupId = "_NA_",
                    IncludeSubCategories = "N",
                    ProductPromoApplEnumId = "PPPA_ALWAYS",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.ProductPromoCategories.AddRangeAsync(productPromoCategories);
            await context.SaveChangesAsync();
        }

        // product promo codes
        if (!context.ProductPromoCodes.Any())
        {
            var productPromoCodes = new List<ProductPromoCode>
            {
                new()
                {
                    ProductPromoCodeId = "9015",
                    ProductPromoId = "9015",
                    UseLimitPerCode = 1,
                    UseLimitPerCustomer = 1,
                    FromDate = nowDateTime,
                    ThruDate = null,
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    ProductPromoCodeId = "9013",
                    ProductPromoId = "9013",
                    UseLimitPerCode = 1,
                    UseLimitPerCustomer = 1,
                    FromDate = nowDateTime,
                    ThruDate = null,
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.ProductPromoCodes.AddRangeAsync(productPromoCodes);
            await context.SaveChangesAsync();
        }

        // crate product store promo appl
        if (!context.ProductStorePromoAppls.Any())
        {
            var productStorePromoAppls = new List<ProductStorePromoAppl>
            {
                new()
                {
                    ProductStoreId = "9000",
                    ProductPromoId = "9011",
                    FromDate = nowDateTime,
                    ThruDate = nowDateTime,
                    SequenceNum = 1,
                    ManualOnly = null,
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    ProductStoreId = "9000",
                    ProductPromoId = "9015",
                    FromDate = nowDateTime,
                    ThruDate = null,
                    SequenceNum = 1,
                    ManualOnly = null,
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.ProductStorePromoAppls.AddRangeAsync(productStorePromoAppls);
            await context.SaveChangesAsync();
        }


        // create party tax auth infos
        if (!context.PartyTaxAuthInfos.Any())
        {
            var partyTaxAuthInfos = new List<PartyTaxAuthInfo>
            {
                new()
                {
                    PartyId = "Company",
                    TaxAuthGeoId = "EGY",
                    TaxAuthPartyId = "EgyptTaxAuth",
                    FromDate = nowDateTime,
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.PartyTaxAuthInfos.AddRangeAsync(partyTaxAuthInfos);


            // select first party id from party for a Customer
            var customerId = context.Parties.FirstOrDefault(a => a.MainRole == "CUSTOMER")?.PartyId;

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
            }

            await context.SaveChangesAsync();
        }

        // create a set of agreements
        if (!context.Agreements.Any())
        {
            // select first party id from party for a Customer
            var customerId1 = context.Parties
                .Where(a => a.MainRole == "CUSTOMER")
                .Skip(1) // Skip the first record
                .Take(1) // Take the next record (second record)
                .FirstOrDefault()?.PartyId;

            var supplierId1 = context.Parties
                .Where(a => a.MainRole == "SUPPLIER")
                .Skip(1) // Skip the first record
                .Take(1) // Take the next record (second record)
                .FirstOrDefault()?.PartyId;

            var agreements = new List<Agreement>
            {
                new()
                {
                    AgreementId = "9000",
                    AgreementTypeId = "SALES_AGREEMENT",
                    PartyIdFrom = "Company",
                    PartyIdTo = customerId1,
                    RoleTypeIdFrom = "INTERNAL_ORGANIZATIO",
                    RoleTypeIdTo = "CUSTOMER",
                    Description = "Sales Agreement",
                    AgreementDate = nowDateTime,
                    FromDate = nowDateTime,
                    ThruDate = null,
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    AgreementId = "9001",
                    AgreementTypeId = "PURCHASE_AGREEMENT",
                    PartyIdFrom = "Company",
                    PartyIdTo = supplierId1,
                    RoleTypeIdFrom = "INTERNAL_ORGANIZATIO",
                    RoleTypeIdTo = "SUPPLIER",
                    Description = "Purchase Agreement",
                    AgreementDate = nowDateTime,
                    FromDate = nowDateTime,
                    ThruDate = null,
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.Agreements.AddRangeAsync(agreements);
            await context.SaveChangesAsync();
        }

        // create agreement items
        if (!context.AgreementItems.Any())
        {
            var agreementItems = new List<AgreementItem>
            {
                new()
                {
                    AgreementId = "9000",
                    AgreementItemSeqId = "01",
                    AgreementItemTypeId = "AGREEMENT_PRICING_PR",
                    CurrencyUomId = "EGP",
                    AgreementText = "Agreement Pricing",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    AgreementId = "9001",
                    AgreementItemSeqId = "01",
                    AgreementItemTypeId = "AGREEMENT_PRICING_PR",
                    CurrencyUomId = "EGP",
                    AgreementText = "Agreement Pricing",
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.AgreementItems.AddRangeAsync(agreementItems);
            await context.SaveChangesAsync();
        }

        // create agreement product appls
        /*if (!context.AgreementProductAppls.Any())
        {
            // select distinct list fro Products PrimaryProductCategoryId
            var productCategories = context.Products
                .Where(x => x.ProductTypeId == "FINISHED_GOOD")
                .Select(x => x.PrimaryProductCategoryId)
                .Distinct()
                .ToList();

            var product01 = context.Products.FirstOrDefault(x => x.PrimaryProductCategoryId == productCategories[0]);
            var product02 = context.Products.FirstOrDefault(x => x.PrimaryProductCategoryId == productCategories[1]);
            var product03 = context.Products.FirstOrDefault(x => x.PrimaryProductCategoryId == productCategories[2]);
            var product04 = context.Products.FirstOrDefault(x => x.PrimaryProductCategoryId == productCategories[3]);

            var agreementProductAppls = new List<AgreementProductAppl>
            {
                new()
                {
                    AgreementId = "9000",
                    AgreementItemSeqId = "01",
                    ProductId = product01.ProductId,
                    Price = 20.00M,
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    AgreementId = "9000",
                    AgreementItemSeqId = "01",
                    ProductId = product02.ProductId,
                    Price = 30.00M,
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    AgreementId = "9001",
                    AgreementItemSeqId = "01",
                    ProductId = product03.ProductId,
                    Price = 40.00M,
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                },
                new()
                {
                    AgreementId = "9001",
                    AgreementItemSeqId = "01",
                    ProductId = product04.ProductId,
                    Price = 50.00M,
                    LastUpdatedStamp = nowDateTime,
                    CreatedStamp = nowDateTime
                }
            };
            await context.AgreementProductAppls.AddRangeAsync(agreementProductAppls);
            await context.SaveChangesAsync();
        }*/
    }
}