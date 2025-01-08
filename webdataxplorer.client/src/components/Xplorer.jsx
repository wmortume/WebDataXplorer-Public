import { useState, useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Button } from "primereact/button";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { InputText } from "primereact/inputtext";
import { FilterMatchMode, FilterOperator } from "primereact/api";
import { IconField } from "primereact/iconfield";
import { InputIcon } from "primereact/inputicon";
import { Calendar } from "primereact/calendar";
import { InputNumber } from "primereact/inputnumber";
import { MultiSelect } from "primereact/multiselect";
import DOMPurify from "dompurify";

function Xplorer() {
    // Default filters for each field which can be manually adjusted
    const defaultFilters = () => {
        return {
            global: { value: null, matchMode: FilterMatchMode.CONTAINS },
            stockQuantity: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.LESS_THAN }]
            },
            itemName: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }]
            },
            location: { value: [], matchMode: FilterMatchMode.IN },
            dateAdded: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
            },
            unitPrice: {
                operator: FilterOperator.OR,
                constraints: [{ value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO }]
            }
        };
    };

    const queryClient = useQueryClient();
    const [filters, setFilters] = useState(defaultFilters());
    const [globalFilterValue, setGlobalFilterValue] = useState("");
    const [expandedRows, setExpandedRows] = useState(null);

    const {
        data: inventoryItems = [],
        isFetching,
        error
    } = useQuery({
        queryFn: async () => {
            const response = await fetch("api/inventory/xplorer");
            if (response.ok) {
                const data = await response.json();
                return data.map((item) => ({
                    ...item,
                    dateAdded: item.dateAdded ? new Date(item.dateAdded) : null
                }));
            } else {
                throw new Error("Failed to fetch inventory items");
            }
        },
        queryKey: ["inventoryItems"],
        refetchInterval: 900000, // 15 minutes
        staleTime: 900000, // Data remains fresh for 15 minutes
        retry: 3, // Retry fetching up to 3 times
        retryDelay: (attempt) => Math.min(1000 * 2 ** attempt, 30000) // Exponential delay for each retry
    });

    const markItemDiscontinued = useMutation({
        mutationFn: async (id) => {
            const response = await fetch(`api/inventory/${id}/discontinued`, {
                method: "PATCH"
            });
            if (!response.ok) {
                throw new Error("Failed to mark item as discontinued");
            }
        },
        onMutate: async (id) => {
            // Cancel outgoing refetches
            await queryClient.cancelQueries({ queryKey: ["inventoryItems"] });

            // Snapshot previous list
            const previousInventoryItems = queryClient.getQueryData(["inventoryItems"]);

            // Optimistically update the list by filtering out the discontinued item
            queryClient.setQueryData(["inventoryItems"], (oldData) =>
                oldData.filter((item) => item.inventoryItemId !== id)
            );
            return { previousInventoryItems };
        },
        onError: (err, id, context) => {
            // Rollback to previous list
            queryClient.setQueryData(["inventoryItems"], context.previousInventoryItems);
        },
        onSettled: () => {
            // Invalidates the list by marking it as stale to refetch data
            queryClient.invalidateQueries({ queryKey: ["inventoryItems"] });
        }
    });

    const locationList = useMemo(
        () => Array.from(new Set(inventoryItems.map((item) => item.location))).filter(Boolean),
        [inventoryItems]
    );

    const onGlobalFilterChange = (e) => {
        const value = e.target.value;
        setGlobalFilterValue(value);

        setFilters((prevFilters) => ({
            ...prevFilters, // ... creates shallow copy and updates only changed properties
            global: { ...prevFilters.global, value }
        }));
    };

    const clearFilter = () => {
        setFilters(defaultFilters());
        setGlobalFilterValue("");
    };

    const markItemDiscontinuedBodyTemplate = (rowData) => {
        return (
            <Button
                label="Discontinued"
                icon="pi pi-trash"
                outlined
                onClick={() => markItemDiscontinued.mutate(rowData.inventoryItemId)}
            />
        );
    };

    const itemNameBodyTemplate = (rowData) => {
        return (
            <div className="flex align-items-center gap-2">
                {rowData.itemImageUrl && (
                    <img src={rowData.itemImageUrl} alt={rowData.itemName} style={{ width: "30px", height: "30px" }} />
                )}
                <span>{rowData.itemName}</span>
            </div>
        );
    };

    const dateAddedFilterTemplate = (options) => {
        return (
            <Calendar
                value={options.value}
                onChange={(e) => options.filterCallback(e.value, options.index)}
                showTime
                hourFormat="12"
                dateFormat="mm/dd/yy"
                placeholder="mm/dd/yyyy hh:mm"
            />
        );
    };

    const priceBodyTemplate = (rowData) => {
        const value = rowData.unitPrice;
        return value ? value.toLocaleString("en-US", { style: "currency", currency: "USD" }) : "";
    };

    const priceFilterTemplate = (options) => {
        return (
            <InputNumber
                value={options.value}
                onChange={(e) => options.filterCallback(e.value, options.index)}
                mode="currency"
                currency="USD"
                locale="en-US"
            />
        );
    };

    const locationFilterTemplate = (options) => {
        return (
            <MultiSelect
                value={options.value || []}
                options={locationList}
                onChange={(e) => options.filterCallback(e.value, options.index)}
                placeholder="Select locations"
                className="p-column-filter"
            />
        );
    };

    const rowExpansionTemplate = (rowData) => {
        const sanitizedHTML = DOMPurify.sanitize(rowData.itemDescription, { FORBID_TAGS: ["button"] }); // Removes buttons and unsafe html tags
        return <div dangerouslySetInnerHTML={{ __html: sanitizedHTML }} />;
    };

    const linkBodyTemplate = (rowData, rowField, linkText) => {
        //Uses rel for security and privacy
        const url = rowData[rowField];
        return url ? (
            <a href={url} target="_blank" rel="noopener noreferrer">
                {linkText}
            </a>
        ) : null;
    };

    const header = (
        <div className="flex justify-content-between">
            <Button type="button" icon="pi pi-filter-slash" label="Clear" outlined onClick={clearFilter} />
            <IconField iconPosition="left">
                <InputIcon className="pi pi-search" />
                <InputText value={globalFilterValue} onChange={onGlobalFilterChange} placeholder="Keyword Search" />
            </IconField>
        </div>
    );

    if (error) {
        return <span>Error: {error.message}</span>;
    }

    return (
        <div className="card">
            <h1>Inventory Items</h1>
            <DataTable
                loading={isFetching}
                value={inventoryItems}
                paginator
                rows={15}
                dataKey="inventoryItemId"
                header={header}
                filterDisplay="menu"
                filters={filters}
                globalFilterFields={["itemName", "itemDescription", "location"]}
                onFilter={(e) => setFilters(e.filters)}
                expandedRows={expandedRows}
                onRowToggle={(e) => setExpandedRows(e.data)}
                rowExpansionTemplate={rowExpansionTemplate}
                emptyMessage="No items found"
                sortField="dateAdded"
                sortOrder={-1}
            >
                <Column expander style={{ width: "5rem" }} />
                <Column header="Item ID" field="inventoryItemId" style={{ minWidth: "8rem" }} />
                <Column
                    header="Mark Discontinued"
                    field="isActive"
                    body={markItemDiscontinuedBodyTemplate}
                    style={{ minWidth: "8rem" }}
                />
                <Column
                    header="Image Source"
                    filterField="url"
                    body={(rowData) => linkBodyTemplate(rowData, "url", "View Source")}
                    style={{ minWidth: "6rem" }}
                />
                <Column
                    header="Stock Quantity"
                    field="stockQuantity"
                    filter
                    dataType="numeric"
                    style={{ minWidth: "10rem" }}
                    sortable
                />
                <Column
                    header="Item Name"
                    field="itemName"
                    filterField="itemName"
                    filter
                    filterPlaceholder="Search by name"
                    body={itemNameBodyTemplate}
                    style={{ minWidth: "14rem" }}
                    sortable
                />
                <Column
                    header="Location"
                    field="location"
                    filter
                    filterElement={locationFilterTemplate}
                    showFilterMatchModes={false}
                    filterMenuStyle={{ width: "20rem" }}
                    style={{ minWidth: "12rem" }}
                    sortable
                />
                <Column
                    header="Date Added"
                    field="dateAdded"
                    filterField="dateAdded"
                    filter
                    filterElement={dateAddedFilterTemplate}
                    body={(rowData) =>
                        rowData.dateAdded ? rowData.dateAdded.toLocaleString("en-US").replace(",", "") : ""
                    }
                    dataType="date"
                    style={{ minWidth: "12rem" }}
                    sortable
                />
                <Column
                    header="Unit Price"
                    field="unitPrice"
                    filterField="unitPrice"
                    filter
                    filterElement={priceFilterTemplate}
                    body={priceBodyTemplate}
                    dataType="numeric"
                    style={{ minWidth: "12rem" }}
                    sortable
                />
            </DataTable>
        </div>
    );
}

export default Xplorer;
