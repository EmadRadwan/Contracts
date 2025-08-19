using Application.Accounting.OrganizationGlSettings;
using Application.Shipments.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting.OrganizationGlSetting;

public class OrganizationGlController : BaseApiController
{
    [HttpGet("{companyId}/getPartyAccountingPreferences")]
    public async Task<IActionResult> GetPartyAccountingPreferences(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetPartyAccountingPreferences.Query { CompanyId = companyId }));
    }

    [HttpGet("{companyId}/getGlAccountTypeDefaults")]
    public async Task<IActionResult> GetGlAccountTypeDefaults(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetGlAccountTypeDefaults.Query { CompanyId = companyId }));
    }

    [HttpGet("{companyId}/getVarianceReasonGlAccounts")]
    public async Task<IActionResult> GetVarianceReasonGlAccounts(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetVarianceReasonGlAccounts.Query { CompanyId = companyId }));
    }

    [HttpGet("{companyId}/getProductCategoryGlAccounts")]
    public async Task<IActionResult> GetProductCategoryGlAccounts(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetProductCategoryGlAccounts.Query { CompanyId = companyId }));
    }

    [HttpGet("{companyId}/getProductGlAccounts")]
    public async Task<IActionResult> GetProductGlAccounts(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetProductGlAccounts.Query { CompanyId = companyId }));
    }

    [HttpGet("{companyId}/getPartyGlAccounts")]
    public async Task<IActionResult> GetPartyGlAccounts(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetPartyGlAccounts.Query { CompanyId = companyId }));
    }

    [HttpGet("{companyId}/getCreditCardTypeGlAccounts")]
    public async Task<IActionResult> GetCreditCardTypeGlAccounts(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetCreditCardTypeGlAccounts.Query { CompanyId = companyId }));
    }

    [HttpGet("{companyId}/getPaymentMethodTypeGlAccounts")]
    public async Task<IActionResult> GetPaymentMethodTypeGlAccounts(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetPaymentMethodTypeGlAccounts.Query { CompanyId = companyId }));
    }

    [HttpGet("{companyId}/getPaymentTypeGlAccountTypes")]
    public async Task<IActionResult> GetPaymentTypeGlAccountTypes(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetPaymentTypeGlAccountTypes.Query { CompanyId = companyId }));
    }

    [HttpGet("{companyId}/getTaxAuthorityGlAccounts")]
    public async Task<IActionResult> GetTaxAuthorityGlAccounts(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetTaxAuthorityGlAccounts.Query { CompanyId = companyId }));
    }

    [HttpGet("{companyId}/getFixedAssetTypeGlAccounts")]
    public async Task<IActionResult> GetFixedAssetTypeGlAccounts(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetFixedAssetTypeGlAccounts.Query { CompanyId = companyId }));
    }

    [HttpGet("{companyId}/getFinAccountTypeGlAccounts")]
    public async Task<IActionResult> GetFinAccountTypeGlAccounts(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetFinAccountTypeGlAccounts.Query { CompanyId = companyId }));
    }

    [HttpGet("{companyId}/getOrganizationGlAccounts")]
    public async Task<IActionResult> GetOrganizationGlAccounts(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetOrganizationGlAccounts.Query { CompanyId = companyId }));
    }

    [HttpGet("getGlAccountHierarchy")]
    public async Task<IActionResult> GlAccountHierarchyView()
    {
        return HandleResult(await Mediator.Send(new GetGlAccountHierarchy.Query()));
    }

    [HttpGet("getGlAccountHierarchyLov")]
    public async Task<IActionResult> GlAccountHierarchyViewLov()
    {
        return HandleResult(await Mediator.Send(new GetGlAccountHierarchyLov.Query()));
    }
    
    [HttpGet("{companyId}/getGlAccountOrganizationHierarchyLov")]
    public async Task<IActionResult> GetGlAccountHierarchy(string companyId)
    {
        var query = new GetGlAccountOrganizationHierarchyLov.Query
        {
            CompanyId = companyId
        };

        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("getGlAccountTypes")]
    public async Task<IActionResult> GetGlAccountTypes()
    {
        return HandleResult(await Mediator.Send(new GetGlAccountTypes.Query()));
    }

    [HttpGet("{companyId}/getGlAccountOrganizationAndClass")]
    public async Task<IActionResult> GetGlAccountOrganizationAndClass(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetGlAccountOrganizationAndClass.Query { CompanyId = companyId }));
    }

    [HttpGet("getGlAccountOrganizationForInvoice")]
    public async Task<IActionResult> GetInvoiceData(
        [FromQuery] string invoiceTypeId,
        [FromQuery] string partyId,
        [FromQuery] string? partyIdFrom)
    {
        var query = new GetGlAccountOrganizationForInvoice.Query
        {
            InvoiceTypeId = invoiceTypeId,
            PartyId = partyId,
            PartyIdFrom = partyIdFrom
        };

        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
    
    [HttpGet("getInvoiceItemTypes")]
    public async Task<IActionResult> GetInvoiceItemTypes(
        [FromQuery] string invoiceTypeId,
        CancellationToken cancellationToken)
    {
        // REFACTOR: Validate only required parameters
        // Purpose: Ensures invoiceTypeId and language are provided
        // Improvement: Simplifies validation, aligns with minimalistic design
        if (string.IsNullOrEmpty(invoiceTypeId))
        {
            return BadRequest("invoiceTypeId and language are required.");
        }

        var query = new GetInvoiceItemTypes.Query
        {
            InvoiceTypeId = invoiceTypeId,
            Language = GetLanguage()
        };

        var result = await Mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return StatusCode(500, new { error = result.Error });
    }

    [HttpGet("getBaseCurrencyId")]
    public async Task<IActionResult> GetBaseCurrencyId()
    {
        return HandleResult(await Mediator.Send(new GetBaseCurrencyId.Query()));
    }

    [HttpGet("{companyId}/getOrganizationGlAccountsByClass")]
    public async Task<IActionResult> GetOrganizationGlAccountsByClass(string companyId, [FromQuery] string Class)
    {
        return HandleResult(await Mediator.Send(new GetOrganizationGlAccountsByClass.Query
            { CompanyId = companyId, Class = Class }));
    }

    [HttpGet("{companyId}/getOrganizationGlAccountsByAccountType")]
    public async Task<IActionResult> GetOrganizationGlAccountsByAccountType(string companyId, [FromQuery] string Type)
    {
        return HandleResult(await Mediator.Send(new GetOrganizationGlAccountsByAccountType.Query
            { CompanyId = companyId, Type = Type }));
    }

    [HttpGet("chart-of-accounts")]
    public async Task<IActionResult> GetChartOfAccounts([FromQuery] string companyId)
    {
        var query = new ListFullOrganizationChartOfAccounts.Query
        {
            CompanyId = companyId
        };

        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Removes a GL Account Type Default using a composite key.
    /// </summary>
    /// <param name="glAccountTypeId">The GL account type identifier.</param>
    /// <param name="organizationPartyId">The organization party identifier.</param>
    /// <param name="glAccountId">The GL account identifier.</param>
    /// <returns>NoContent on success or an error result.</returns>
    [HttpDelete("{glAccountTypeId}/{organizationPartyId}/{glAccountId}")]
    public async Task<IActionResult> RemoveGlAccountTypeDefault(string glAccountTypeId, string organizationPartyId,
        string glAccountId)
    {
        var command = new RemoveGlAccountTypeDefault.Command
        {
            GlAccountTypeId = glAccountTypeId,
            OrganizationPartyId = organizationPartyId,
            GlAccountId = glAccountId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            // For example, if the error indicates the record wasn't found, return NotFound.
            if (result.Error == "Record not found")
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return NoContent();
    }


    /// <summary>
    /// Deletes a Product GL Account using a composite key.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="organizationPartyId">The organization party identifier.</param>
    /// <param name="glAccountTypeId">The GL account type identifier.</param>
    /// <returns>NoContent if deletion is successful, or an appropriate error response.</returns>
    [HttpDelete("{productId}/{organizationPartyId}/{glAccountTypeId}")]
    public async Task<IActionResult> DeleteProductGlAccount(string productId, string organizationPartyId,
        string glAccountTypeId)
    {
        var command = new DeleteProductGlAccount.Command
        {
            ProductId = productId,
            OrganizationPartyId = organizationPartyId,
            GlAccountTypeId = glAccountTypeId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Record not found")
                return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a Product Category GL Account using a composite key.
    /// </summary>
    /// <param name="productCategoryId">The product category identifier.</param>
    /// <param name="organizationPartyId">The organization party identifier.</param>
    /// <param name="glAccountTypeId">The GL account type identifier.</param>
    /// <returns>NoContent on success, NotFound if the record is not found, or BadRequest on error.</returns>
    [HttpDelete("{productCategoryId}/{organizationPartyId}/{glAccountTypeId}")]
    public async Task<IActionResult> DeleteProductCategoryGlAccount(string productCategoryId,
        string organizationPartyId, string glAccountTypeId)
    {
        var command = new DeleteProductCategoryGlAccount.Command
        {
            ProductCategoryId = productCategoryId,
            OrganizationPartyId = organizationPartyId,
            GlAccountTypeId = glAccountTypeId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Record not found")
                return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a FinAccountTypeGlAccount using a composite key.
    /// </summary>
    /// <param name="finAccountTypeId">The Fin Account Type identifier.</param>
    /// <param name="organizationPartyId">The Organization Party identifier.</param>
    /// <returns>NoContent on success, NotFound if the record is not found, or BadRequest on error.</returns>
    [HttpDelete("{finAccountTypeId}/{organizationPartyId}")]
    public async Task<IActionResult> DeleteFinAccountTypeGlAccount(string finAccountTypeId, string organizationPartyId)
    {
        var command = new DeleteFinAccountTypeGlAccount.Command
        {
            FinAccountTypeId = finAccountTypeId,
            OrganizationPartyId = organizationPartyId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Record not found")
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Removes a Payment GL Account Type Map using a composite key.
    /// </summary>
    /// <param name="paymentTypeId">The payment type identifier.</param>
    /// <param name="organizationPartyId">The organization party identifier.</param>
    /// <returns>NoContent on success, NotFound if the record is not found, or BadRequest on error.</returns>
    [HttpDelete("{paymentTypeId}/{organizationPartyId}")]
    public async Task<IActionResult> RemovePaymentTypeGlAssignment(string paymentTypeId, string organizationPartyId)
    {
        var command = new RemovePaymentTypeGlAssignment.Command
        {
            PaymentTypeId = paymentTypeId,
            OrganizationPartyId = organizationPartyId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Record not found")
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Removes a Payment Method Type GL Assignment using a composite key.
    /// </summary>
    /// <param name="paymentMethodTypeId">The payment method type identifier.</param>
    /// <param name="organizationPartyId">The organization party identifier.</param>
    /// <returns>NoContent on success, NotFound if the record is not found, or BadRequest on error.</returns>
    [HttpDelete("{paymentMethodTypeId}/{organizationPartyId}")]
    public async Task<IActionResult> RemovePaymentMethodTypeGlAssignment(string paymentMethodTypeId,
        string organizationPartyId)
    {
        var command = new RemovePaymentMethodTypeGlAssignment.Command
        {
            PaymentMethodTypeId = paymentMethodTypeId,
            OrganizationPartyId = organizationPartyId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Record not found")
                return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }


    /// <summary>
    /// Deletes a Variance Reason GL Account using a composite key.
    /// </summary>
    /// <param name="varianceReasonId">The Variance Reason identifier.</param>
    /// <param name="organizationPartyId">The Organization Party identifier.</param>
    /// <returns>NoContent on success, NotFound if the record is not found, or BadRequest on error.</returns>
    [HttpDelete("{varianceReasonId}/{organizationPartyId}")]
    public async Task<IActionResult> DeleteVarianceReasonGlAccount(string varianceReasonId, string organizationPartyId)
    {
        var command = new DeleteVarianceReasonGlAccount.Command
        {
            VarianceReasonId = varianceReasonId,
            OrganizationPartyId = organizationPartyId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Record not found")
                return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a Credit Card GL Account using a composite key.
    /// </summary>
    /// <param name="cardType">The credit card type.</param>
    /// <param name="organizationPartyId">The organization party identifier.</param>
    /// <returns>NoContent on success, NotFound if the record is not found, or BadRequest on error.</returns>
    [HttpDelete("{cardType}/{organizationPartyId}")]
    public async Task<IActionResult> DeleteCreditCardTypeGlAccount(string cardType, string organizationPartyId)
    {
        var command = new DeleteCreditCardTypeGlAccount.Command
        {
            CardType = cardType,
            OrganizationPartyId = organizationPartyId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Record not found")
                return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing Party GL Account using a composite key.
    /// </summary>
    /// <param name="organizationPartyId">The organization party identifier.</param>
    /// <param name="partyId">The party identifier.</param>
    /// <param name="roleTypeId">The role type identifier.</param>
    /// <param name="glAccountTypeId">The GL account type identifier.</param>
    /// <returns>NoContent on success, NotFound if the record is not found, or BadRequest on error.</returns>
    [HttpDelete("{organizationPartyId}/{partyId}/{roleTypeId}/{glAccountTypeId}")]
    public async Task<IActionResult> DeletePartyGlAccount(string organizationPartyId, string partyId, string roleTypeId,
        string glAccountTypeId)
    {
        var command = new DeletePartyGlAccount.Command
        {
            OrganizationPartyId = organizationPartyId,
            PartyId = partyId,
            RoleTypeId = roleTypeId,
            GlAccountTypeId = glAccountTypeId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Record not found")
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a Fixed Asset Type GL Account Mapping using a composite key.
    /// </summary>
    /// <param name="finAccountTypeId">The Fixed Asset Type identifier.</param>
    /// <param name="organizationPartyId">The Organization Party identifier.</param>
    /// <returns>NoContent on success, NotFound if the record is not found, or BadRequest on error.</returns>
    [HttpDelete("{finAccountTypeId}/{organizationPartyId}")]
    public async Task<IActionResult> DeleteFixedAssetTypeGlAccount(string finAccountTypeId, string organizationPartyId)
    {
        var command = new DeleteFixedAssetTypeGlAccount.Command
        {
            FinAccountTypeId = finAccountTypeId,
            OrganizationPartyId = organizationPartyId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Record not found")
                return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }
}