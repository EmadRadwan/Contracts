<simple-method method-name="updateAcctgTransEntry" short-description="Update Entry To AcctgTrans">
    <entity-one entity-name="AcctgTransEntry" value-field="lookedUpValue"/>
    <!-- Only status change will be allowed in case of posted entry -->
<make-value entity-name="AcctgTransEntry" value-field="acctgTransEntry"/>
<set field="acctgTransEntry" from-field="lookedUpValue"/>
<set-nonpk-fields map="parameters" value-field="acctgTransEntry"/>
<set field="lookedUpValue.reconcileStatusId" from-field="acctgTransEntry.reconcileStatusId"/>
<if-compare-field field="acctgTransEntry" operator="not-equals" to-field="lookedUpValue">
<entity-one entity-name="AcctgTrans" value-field="acctgTrans"/>
<if-compare field="acctgTrans.isPosted" operator="equals" value="Y">
<add-error><fail-property resource="AccountingUiLabels" property="AccountingTransactionHasBeenAlreadyPosted"/></add-error>
<check-errors/>
</if-compare>
</if-compare-field>
<set-nonpk-fields map="parameters" value-field="lookedUpValue"/>
<store-value value-field="lookedUpValue"/>

<!-- when changing entries, also update the last modified info for the AcctgTrans -->
<call-simple-method method-name="updateAcctgTransLastModified"/>
</simple-method>