let dashboardRole = "";

document.addEventListener("DOMContentLoaded", () => {
	initializeDashboard();
});

async function initializeDashboard() {
	dashboardRole = localStorage.getItem("userRole") || "";

	setDefaultDates();
	setupFilterEvents();
	setupSectionNavigation();

	// Reports/Dashboard data is intentionally restricted to Admin and Service Advisor.
	if (!canViewManagementDashboard()) {
		showRestrictedDashboard();
		return;
	}

	await loadDashboard();
}

function canViewManagementDashboard() {
	return dashboardRole === "Admin" || dashboardRole === "Service Advisor";
}

function showRestrictedDashboard() {
	hideLoading();

	document.getElementById("dashboardSectionNav")?.classList.add("d-none");
	document.querySelector(".dashboard-filter")?.classList.add("d-none");

	const content = document.getElementById("dashboardContent");

	if (content) {
		content.innerHTML = `
            <section class="section-panel text-center py-5">
                <i class="bi bi-bar-chart-line fs-1 text-primary"></i>
                <h4 class="mt-3">Dashboard Summary</h4>
                <p class="text-muted mb-0">
                    Detailed management dashboard information is available to Admin and Service Advisor users.
                </p>
            </section>
        `;
		content.classList.remove("d-none");
	}
}

function setDefaultDates() {
	const now = new Date();
	const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
	const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());

	document.getElementById("fromDate").value = formatDateForInput(firstDay);
	document.getElementById("toDate").value = formatDateForInput(today);

	updatePeriodLabel();
}

function setupFilterEvents() {
	document
		.getElementById("applyFilterButton")
		?.addEventListener("click", loadDashboard);

	document
		.getElementById("resetFilterButton")
		?.addEventListener("click", async () => {
			setDefaultDates();
			await loadDashboard();
		});
}

function setupSectionNavigation() {
	document.querySelectorAll(".dashboard-section-nav a").forEach((link) => {
		link.addEventListener("click", () => {
			document
				.querySelectorAll(".dashboard-section-nav a")
				.forEach((item) => item.classList.remove("active"));

			link.classList.add("active");
		});
	});
}

async function loadDashboard() {
	const fromDate = document.getElementById("fromDate").value;
	const toDate = document.getElementById("toDate").value;

	if (!fromDate || !toDate) {
		showDashboardAlert(
			"Please select both From Date and To Date.",
			"warning",
		);
		return;
	}

	if (fromDate > toDate) {
		showDashboardAlert(
			"From Date must be earlier than or equal to To Date.",
			"warning",
		);
		return;
	}

	updatePeriodLabel();
	showLoading();
	hideDashboardAlert();

	try {
		const query = `?fromDate=${encodeURIComponent(fromDate)}&toDate=${encodeURIComponent(toDate)}`;

		const [revenue, services, vehicles, inventory] = await Promise.all([
			apiGet(`/Reports/revenue${query}`),
			apiGet(`/Reports/services${query}`),
			apiGet("/Reports/vehicles"),
			apiGet("/Reports/inventory"),
		]);

		renderRevenue(revenue);
		renderServices(services);
		renderVehicles(vehicles);
		renderInventory(inventory);

		hideLoading();
	} catch (error) {
		console.error("Dashboard loading error:", error);
		hideLoading();
		showDashboardAlert(
			error.message || "Unable to load dashboard data.",
			"danger",
		);
	}
}

function renderRevenue(data) {
	const total = Number(data.totalInvoicedRevenue || 0);
	const paid = Number(data.totalPaidRevenue || 0);
	const unpaid = Number(data.totalUnpaidRevenue || 0);
	const collectionRate = total > 0 ? (paid / total) * 100 : 0;

	setText("overviewInvoices", data.totalInvoices ?? 0);
	setText("overviewRevenue", formatCurrency(total));
	setText("overviewPaidRevenue", formatCurrency(paid));
	setText("overviewUnpaidRevenue", formatCurrency(unpaid));

	setText("revenueTotal", formatCurrency(total));
	setText("revenuePaid", formatCurrency(paid));
	setText("revenueUnpaid", formatCurrency(unpaid));
	setText("revenueCollectionRate", `${collectionRate.toFixed(1)}%`);

	setText("totalInvoiceCount", data.totalInvoices ?? 0);
	setText("paidInvoiceCount", data.paidInvoices ?? 0);
	setText("unpaidInvoiceCount", data.unpaidInvoices ?? 0);

	setText("revenuePeriodLabel", formatPeriodFromInputs());
}

function renderServices(data) {
	setText("overviewServiceJobs", data.totalServiceJobs ?? 0);
	renderServiceChart(data.byStatus || []);
	renderServiceTypes(data.byServiceType || []);
}

function renderServiceChart(statuses) {
	const container = document.getElementById("serviceChart");
	container.innerHTML = "";

	if (!statuses.length) {
		container.innerHTML = `<div class="text-muted align-self-center mx-auto py-5">No service jobs found for the selected period.</div>`;
		return;
	}

	const maxCount = Math.max(
		...statuses.map((item) => Number(item.count || 0)),
		1,
	);

	statuses.forEach((item) => {
		const count = Number(item.count || 0);
		const height = count === 0 ? 2 : Math.max((count / maxCount) * 230, 8);
		const label = formatStatusLabel(item.status);

		container.innerHTML += `
            <div class="service-chart-column">
                <div class="service-chart-value">${count}</div>
                <div class="service-chart-bar" style="height: ${height}px;" title="${escapeHtml(label)}: ${count}"></div>
                <div class="service-chart-label">${escapeHtml(label)}</div>
            </div>
        `;
	});
}

function renderServiceTypes(serviceTypes) {
	const container = document.getElementById("serviceTypeDetails");
	container.innerHTML = "";

	if (!serviceTypes.length) {
		container.innerHTML = `<div class="text-muted">No service type data available.</div>`;
		return;
	}

	serviceTypes.forEach((item) => {
		container.innerHTML += `
            <div class="service-detail-item">
                <span>${escapeHtml(item.serviceTypeName)}</span>
                <strong>${item.serviceJobCount}</strong>
            </div>
        `;
	});
}

function renderVehicles(data) {
	setText("overviewVehicles", data.totalVehicles ?? 0);
	setText("vehicleTotal", data.totalVehicles ?? 0);
	setText("vehicleServiced", data.servicedVehicles ?? 0);
	setText("vehicleNeverServiced", data.neverServicedVehicles ?? 0);

	renderVehicleList("vehicleMakes", data.byMake || [], "make");
	renderVehicleList("vehicleFuelTypes", data.byFuelType || [], "fuelType");
}

function renderVehicleList(elementId, items, propertyName) {
	const container = document.getElementById(elementId);
	container.innerHTML = "";

	if (!items.length) {
		container.innerHTML = `<span class="text-muted">No data available.</span>`;
		return;
	}

	items.forEach((item) => {
		container.innerHTML += `
            <div class="detail-list-item">
                ${escapeHtml(item[propertyName])}
                <strong class="ms-2">${item.count}</strong>
            </div>
        `;
	});
}

function renderInventory(data) {
	setText("inventoryParts", data.totalActiveParts ?? 0);
	setText("inventoryQuantity", data.totalQuantity ?? 0);
	setText("inventoryLowStock", data.lowStockParts ?? 0);
	setText("inventoryOutOfStock", data.outOfStockParts ?? 0);
	setText("inventoryValue", formatCurrency(data.inventoryValue));

	renderLowStockParts(data.lowStockItems || []);
}

function renderLowStockParts(parts) {
	const table = document.getElementById("lowStockTable");
	const emptyMessage = document.getElementById("noLowStock");

	table.innerHTML = "";

	if (!parts.length) {
		emptyMessage.classList.remove("d-none");
		return;
	}

	emptyMessage.classList.add("d-none");

	parts.forEach((part) => {
		const status = part.isOutOfStock
			? `<span class="badge bg-danger">Out of Stock</span>`
			: `<span class="badge bg-warning text-dark">Low Stock</span>`;

		table.innerHTML += `
            <tr>
                <td>${escapeHtml(part.partNumber)}</td>
                <td>${escapeHtml(part.name)}</td>
                <td><strong>${part.quantity}</strong></td>
                <td>${part.minimumStock}</td>
                <td>${formatCurrency(part.unitPrice)}</td>
                <td>${status}</td>
            </tr>
        `;
	});
}

function updatePeriodLabel() {
	const label = formatPeriodFromInputs();
	setText("selectedPeriod", label);
	setText("revenuePeriodLabel", label);
}

function formatPeriodFromInputs() {
	const from = document.getElementById("fromDate")?.value;
	const to = document.getElementById("toDate")?.value;

	if (!from || !to) return "";

	return buildPeriodLabel(from, to);
}

function buildPeriodLabel(fromDate, toDate) {
	const from = parseLocalDate(fromDate);
	const to = parseLocalDate(toDate);

	if (
		from.getMonth() === to.getMonth() &&
		from.getFullYear() === to.getFullYear()
	) {
		return from.toLocaleDateString("en-IN", {
			month: "long",
			year: "numeric",
		});
	}

	return `${from.toLocaleDateString("en-IN", {
		day: "numeric",
		month: "long",
		year: "numeric",
	})} - ${to.toLocaleDateString("en-IN", {
		day: "numeric",
		month: "long",
		year: "numeric",
	})}`;
}

function parseLocalDate(value) {
	const [year, month, day] = value.split("-").map(Number);
	return new Date(year, month - 1, day);
}

function formatStatusLabel(status) {
	const labels = {
		Scheduled: "Scheduled",
		Received: "Received",
		Inspection: "Inspection",
		InProgress: "In Progress",
		Completed: "Completed",
		ReadyForPickup: "Ready for Pickup",
		Delivered: "Delivered",
	};

	return labels[status] || status;
}

function formatCurrency(value) {
	return Number(value ?? 0).toLocaleString("en-IN", {
		style: "currency",
		currency: "INR",
		maximumFractionDigits: 2,
	});
}

function formatDateForInput(date) {
	const year = date.getFullYear();
	const month = String(date.getMonth() + 1).padStart(2, "0");
	const day = String(date.getDate()).padStart(2, "0");
	return `${year}-${month}-${day}`;
}

function setText(elementId, value) {
	const element = document.getElementById(elementId);
	if (element) element.textContent = value;
}

function escapeHtml(value) {
	if (value === null || value === undefined) return "";

	return String(value)
		.replace(/&/g, "&amp;")
		.replace(/</g, "&lt;")
		.replace(/>/g, "&gt;")
		.replace(/"/g, "&quot;")
		.replace(/'/g, "&#039;");
}

function showLoading() {
	document.getElementById("dashboardLoading")?.classList.remove("d-none");
	document.getElementById("dashboardContent")?.classList.add("d-none");
}

function hideLoading() {
	document.getElementById("dashboardLoading")?.classList.add("d-none");
	document.getElementById("dashboardContent")?.classList.remove("d-none");
}

function showDashboardAlert(message, type = "danger") {
	const alertElement = document.getElementById("dashboardAlert");
	if (!alertElement) return;

	alertElement.className = `alert alert-${type} mb-4`;
	alertElement.textContent = message;
}

function hideDashboardAlert() {
	const alertElement = document.getElementById("dashboardAlert");
	if (!alertElement) return;

	alertElement.className = "alert d-none mb-4";
	alertElement.textContent = "";
}
