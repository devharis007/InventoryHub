// InventoryHub dashboard view model
// Powers the reactive product list, dashboard stats, search/filter, and
// stock-adjustment UI using Knockout.js observables bound to JSON API
// endpoints exposed by the ASP.NET Core MVC controllers under /api.
(function () {
    "use strict";

    var API = {
        products: "/api/products",
        categories: "/api/categories",
        stockMovements: "/api/stockmovements"
    };

    function currency(value) {
        var n = Number(value) || 0;
        return "$" + n.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // Wraps a raw product DTO from the API into an observable-backed row.
    function ProductRow(dto) {
        var self = this;
        self.id = dto.id;
        self.name = ko.observable(dto.name);
        self.sku = ko.observable(dto.sku);
        self.categoryId = ko.observable(dto.categoryId);
        self.categoryName = ko.observable(dto.categoryName);
        self.price = ko.observable(dto.price);
        self.quantityInStock = ko.observable(dto.quantityInStock);
        self.reorderThreshold = ko.observable(dto.reorderThreshold);

        self.priceFormatted = ko.pureComputed(function () { return currency(self.price()); });
        self.isLowStock = ko.pureComputed(function () { return self.quantityInStock() < self.reorderThreshold(); });

        self.updateFrom = function (updatedDto) {
            self.name(updatedDto.name);
            self.sku(updatedDto.sku);
            self.categoryId(updatedDto.categoryId);
            self.categoryName(updatedDto.categoryName);
            self.price(updatedDto.price);
            self.quantityInStock(updatedDto.quantityInStock);
            self.reorderThreshold(updatedDto.reorderThreshold);
        };
    }

    function CategoryRow(dto) {
        this.id = dto.id;
        this.name = dto.name;
        this.productCount = dto.productCount;
    }

    function emptyFormModel() {
        return {
            id: ko.observable(null),
            name: ko.observable(""),
            sku: ko.observable(""),
            categoryId: ko.observable(null),
            price: ko.observable(0),
            reorderThreshold: ko.observable(0),
            quantityInStock: ko.observable(0)
        };
    }

    function emptyAdjustModel() {
        return {
            productId: ko.observable(null),
            productName: ko.observable(""),
            currentQuantity: ko.observable(0),
            changeAmount: ko.observable(0),
            reason: ko.observable("Restock")
        };
    }

    function DashboardViewModel() {
        var self = this;

        // --- Core data ---
        self.products = ko.observableArray([]);
        self.categories = ko.observableArray([]);

        // --- Filters ---
        self.searchTerm = ko.observable("");
        self.selectedCategoryId = ko.observable(null);
        self.showLowStockOnly = ko.observable(false);

        // --- UI state ---
        self.errorMessage = ko.observable("");
        self.formError = ko.observable("");
        self.isEditing = ko.observable(false);
        self.formModel = ko.observable(emptyFormModel());
        self.adjustModel = ko.observable(emptyAdjustModel());
        self.adjustmentReasons = ["Restock", "Sale", "Adjustment", "Damaged"];

        // --- Computed: filtered product list ---
        self.filteredProducts = ko.pureComputed(function () {
            var term = (self.searchTerm() || "").trim().toLowerCase();
            var categoryId = self.selectedCategoryId();
            var lowOnly = self.showLowStockOnly();

            return ko.utils.arrayFilter(self.products(), function (p) {
                if (term) {
                    var matches = p.name().toLowerCase().indexOf(term) !== -1 ||
                        p.sku().toLowerCase().indexOf(term) !== -1;
                    if (!matches) return false;
                }
                if (categoryId && Number(p.categoryId()) !== Number(categoryId)) return false;
                if (lowOnly && !p.isLowStock()) return false;
                return true;
            });
        });

        // --- Computed: dashboard stats (based on the full product set, not the filtered view) ---
        self.totalProducts = ko.pureComputed(function () { return self.products().length; });

        self.totalStockValue = ko.pureComputed(function () {
            var total = 0;
            ko.utils.arrayForEach(self.products(), function (p) {
                total += p.price() * p.quantityInStock();
            });
            return total;
        });
        self.totalStockValueFormatted = ko.pureComputed(function () { return currency(self.totalStockValue()); });

        self.lowStockCount = ko.pureComputed(function () {
            return ko.utils.arrayFilter(self.products(), function (p) { return p.isLowStock(); }).length;
        });

        // --- Data loading ---
        self.loadCategories = function () {
            return fetch(API.categories)
                .then(function (r) { if (!r.ok) throw new Error("Failed to load categories."); return r.json(); })
                .then(function (data) {
                    self.categories(ko.utils.arrayMap(data, function (c) { return new CategoryRow(c); }));
                })
                .catch(function (err) { self.errorMessage(err.message); });
        };

        self.loadProducts = function () {
            return fetch(API.products)
                .then(function (r) { if (!r.ok) throw new Error("Failed to load products."); return r.json(); })
                .then(function (data) {
                    self.products(ko.utils.arrayMap(data, function (p) { return new ProductRow(p); }));
                })
                .catch(function (err) { self.errorMessage(err.message); });
        };

        // --- Add / Edit modal ---
        var productModalEl, adjustModalEl;

        self.openAddModal = function () {
            self.formError("");
            self.isEditing(false);
            self.formModel(emptyFormModel());
            if (self.categories().length) {
                self.formModel().categoryId(self.categories()[0].id);
            }
            productModalEl.show();
        };

        self.openEditModal = function (product) {
            self.formError("");
            self.isEditing(true);
            var m = emptyFormModel();
            m.id(product.id);
            m.name(product.name());
            m.sku(product.sku());
            m.categoryId(product.categoryId());
            m.price(product.price());
            m.reorderThreshold(product.reorderThreshold());
            m.quantityInStock(product.quantityInStock());
            self.formModel(m);
            productModalEl.show();
        };

        self.saveProduct = function () {
            var m = self.formModel();
            var payload = {
                name: m.name(),
                sku: m.sku(),
                categoryId: Number(m.categoryId()),
                price: parseFloat(m.price()) || 0,
                reorderThreshold: parseInt(m.reorderThreshold(), 10) || 0,
                quantityInStock: parseInt(m.quantityInStock(), 10) || 0
            };

            var isEdit = self.isEditing();
            var url = isEdit ? API.products + "/" + m.id() : API.products;
            var method = isEdit ? "PUT" : "POST";

            fetch(url, {
                method: method,
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            })
                .then(function (r) {
                    if (!r.ok) return r.text().then(function (t) { throw new Error(t || "Unable to save product."); });
                    return r.json();
                })
                .then(function (savedDto) {
                    if (isEdit) {
                        var existing = ko.utils.arrayFirst(self.products(), function (p) { return p.id === savedDto.id; });
                        if (existing) existing.updateFrom(savedDto);
                    } else {
                        self.products.push(new ProductRow(savedDto));
                    }
                    self.formError("");
                    productModalEl.hide();
                })
                .catch(function (err) { self.formError(err.message); });
        };

        self.confirmDeleteProduct = function (product) {
            if (!window.confirm('Delete "' + product.name() + '"? This cannot be undone.')) return;

            fetch(API.products + "/" + product.id, { method: "DELETE" })
                .then(function (r) {
                    if (!r.ok) throw new Error("Unable to delete product.");
                    self.products.remove(product);
                })
                .catch(function (err) { self.errorMessage(err.message); });
        };

        // --- Stock adjustment ---
        self.applyAdjustment = function (product, changeAmount, reason) {
            self.errorMessage("");
            var payload = { productId: product.id, changeAmount: changeAmount, reason: reason };

            return fetch(API.stockMovements, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            })
                .then(function (r) {
                    if (!r.ok) return r.text().then(function (t) { throw new Error(t || "Unable to adjust stock."); });
                    return r.json();
                })
                .then(function () {
                    product.quantityInStock(product.quantityInStock() + changeAmount);
                });
        };

        self.incrementStock = function (product) {
            self.applyAdjustment(product, 1, "Restock").catch(function (err) { self.errorMessage(err.message); });
        };

        self.decrementStock = function (product) {
            if (product.quantityInStock() <= 0) return;
            self.applyAdjustment(product, -1, "Sale").catch(function (err) { self.errorMessage(err.message); });
        };

        self.openAdjustModal = function (product) {
            self.formError("");
            var m = emptyAdjustModel();
            m.productId(product.id);
            m.productName(product.name());
            m.currentQuantity(product.quantityInStock());
            self.adjustModel(m);
            adjustModalEl.show();
        };

        self.submitAdjustment = function () {
            var m = self.adjustModel();
            var change = parseInt(m.changeAmount(), 10);
            if (!change) {
                self.formError("Enter a non-zero change amount.");
                return;
            }
            var product = ko.utils.arrayFirst(self.products(), function (p) { return p.id === m.productId(); });
            if (!product) { self.formError("Product not found."); return; }

            self.applyAdjustment(product, change, m.reason())
                .then(function () {
                    self.formError("");
                    adjustModalEl.hide();
                })
                .catch(function (err) { self.formError(err.message); });
        };

        // --- Init ---
        self.init = function () {
            var productModalNode = document.getElementById("productModal");
            var adjustModalNode = document.getElementById("adjustModal");
            productModalEl = new bootstrap.Modal(productModalNode);
            adjustModalEl = new bootstrap.Modal(adjustModalNode);

            Promise.all([self.loadCategories(), self.loadProducts()]).then(function () {
                document.getElementById("dashboard-app").style.display = "";
            });
        };
    }

    document.addEventListener("DOMContentLoaded", function () {
        var root = document.getElementById("dashboard-app");
        if (!root) return;

        var vm = new DashboardViewModel();
        ko.applyBindings(vm, root);
        vm.init();
    });
})();
