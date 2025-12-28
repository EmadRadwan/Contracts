<ApartmentHeaderSection
  ...
onProductChange={(form, e) => handleProductChange(form, e, setSelectedApartment)}
/>

<PricingSection
  ...
onPricePerM2Change={handlePricePerM2Change}
onDiscountChange={handleDiscountChange}
autoSetDerivedFields={autoSetDerivedFields}
/>

<PaymentFieldsSection
  ...
onAdvanceChange={handleAdvanceChange}
/>