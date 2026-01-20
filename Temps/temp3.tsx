<KendoGrid
    className="main-grid"
    data={processedData}           // ← now use processedData
    sortable={true}
    resizable={true}
    sort={sort}
    onSortChange={(e) => setSort(e.sort)}

    filterable={true}              // ← enable filter row
    filter={filter}                // ← controlled filter
    onFilterChange={onFilterChange}

    skip={page.skip}
    take={page.take}
    total={total}                  // ← important!
    pageable={true}
    onPageChange={pageChange}
>
    {/* Toolbar + Columns stay the same */}
</KendoGrid>