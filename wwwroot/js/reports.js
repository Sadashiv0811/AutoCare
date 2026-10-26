let reportData = null;
let reportPage = 1;
let reportLoadingRequest = false;
const reportPerPage = 5;

const reportElements = {};

function getReportElements() {
	[
		"generateReport",
		"resetReport",
		"exportReport",
		"reportFrom",
		"reportTo",
		"reportAlert",
		"reportLoading",
		"reportContent",
		"rRevenue",
		"rPaid",
		"rInvoices",
		"rJobs",
		"monthlyRevenueBody",
		"reportInfo",
		"reportPages",
		"statusReport",
		"serviceTypeReport",
		"makeReport",
		"vehicleCountReport",
		"fuelReport",
		"invActive",
		"invQty",
		"invLow",
		"invEmpty",
		"invValue",
		"lowStockReportBody",
	].forEach((id) => (reportElements[id] = document.getElementById(id)));
	return reportElements;
}

document.addEventListener("DOMContentLoaded", () => {
	getReportElements();
	setReportDates();
	reportElements.exportReport.disabled = true;

	reportElements.generateReport.addEventListener("click", loadReports);
	reportElements.resetReport.addEventListener("click", () => {
		setReportDates();
		reportPage = 1;
		loadReports();
	});
	reportElements.exportReport.addEventListener("click", exportReports);
	reportElements.reportFrom.addEventListener("change", () => {
		reportPage = 1;
	});
	reportElements.reportTo.addEventListener("change", () => {
		reportPage = 1;
	});
	loadReports();
});

function formatDateInput(date) {
	const year = date.getFullYear();
	const month = String(date.getMonth() + 1).padStart(2, "0");
	const day = String(date.getDate()).padStart(2, "0");
	return `${year}-${month}-${day}`;
}

function setReportDates() {
	const d = new Date();
	const first = new Date(d.getFullYear(), d.getMonth(), 1);
	reportElements.reportFrom.value = formatDateInput(first);
	reportElements.reportTo.value = formatDateInput(d);
}

function validateReportDates() {
	const errors = [];

	if (!reportElements.reportFrom.value) errors.push("From Date is required.");
	if (!reportElements.reportTo.value) errors.push("To Date is required.");

	if (
		reportElements.reportFrom.value &&
		reportElements.reportTo.value &&
		reportElements.reportFrom.value > reportElements.reportTo.value
	) {
		errors.push("From Date must be earlier than or equal to To Date.");
	}

	return errors;
}

async function loadReports() {
	if (reportLoadingRequest) return;
	const errors = validateReportDates();
	if (errors.length) {
		reportElements.reportAlert.innerHTML = errors
			.map((x) => `<div>${esc(x)}</div>`)
			.join("");
		reportElements.reportAlert.className = "alert alert-warning";
		reportElements.reportAlert.classList.remove("d-none");
		reportElements.reportFrom.classList.toggle(
			"is-invalid",
			!reportElements.reportFrom.value ||
				reportElements.reportFrom.value > reportElements.reportTo.value,
		);
		reportElements.reportTo.classList.toggle(
			"is-invalid",
			!reportElements.reportTo.value ||
				reportElements.reportFrom.value > reportElements.reportTo.value,
		);
		return;
	}

	reportElements.reportAlert.classList.add("d-none");
	reportElements.reportFrom.classList.remove("is-invalid");
	reportElements.reportTo.classList.remove("is-invalid");
	reportLoadingRequest = true;
	reportElements.generateReport.disabled = true;
	reportElements.exportReport.disabled = true;
	reportElements.reportLoading.classList.remove("d-none");
	reportElements.reportContent.classList.add("opacity-50");

	try {
		const q = `?fromDate=${encodeURIComponent(reportElements.reportFrom.value)}&toDate=${encodeURIComponent(reportElements.reportTo.value)}`;
		const [revenue, services, vehicles, inventory] = await Promise.all([
			apiGet(`/reports/revenue${q}`),
			apiGet(`/reports/services${q}`),
			apiGet("/reports/vehicles"),
			apiGet("/reports/inventory"),
		]);
		reportData = { revenue, services, vehicles, inventory };
		reportPage = 1;
		renderReport();
		reportElements.reportAlert.classList.add("d-none");
	} catch (e) {
		reportElements.reportAlert.textContent =
			e?.message || "Unable to load reports. Please try again.";
		reportElements.reportAlert.className = "alert alert-danger";
		reportElements.reportAlert.classList.remove("d-none");
	} finally {
		reportLoadingRequest = false;
		reportElements.generateReport.disabled = false;
		reportElements.exportReport.disabled = !reportData;
		reportElements.reportLoading.classList.add("d-none");
		reportElements.reportContent.classList.remove("opacity-50");
	}
}

function renderReport() {
	const { revenue: r, services: s, vehicles: v, inventory: i } = reportData;

	reportElements.rRevenue.textContent = money(r.totalInvoicedRevenue);
	reportElements.rPaid.textContent = money(r.totalPaidRevenue);
	reportElements.rInvoices.textContent = r.totalInvoices;
	reportElements.rJobs.textContent = s.totalServiceJobs;

	const monthly = r.monthlyRevenue || [];
	const pages = Math.max(1, Math.ceil(monthly.length / reportPerPage));
	if (reportPage > pages) reportPage = pages;

	const page = monthly.slice(
		(reportPage - 1) * reportPerPage,
		reportPage * reportPerPage,
	);

	reportElements.monthlyRevenueBody.innerHTML =
		page
			.map(
				(x) =>
					`<tr><td>${esc(x.monthName)}</td><td>${x.invoiceCount}</td><td>${money(x.invoicedRevenue)}</td><td>${money(x.paidRevenue)}</td><td>${x.unpaidInvoiceCount}</td></tr>`,
			)
			.join("") ||
		'<tr><td colspan="5" class="text-center text-muted py-4">No revenue data.</td></tr>';

	reportElements.reportInfo.textContent = `Showing ${
		monthly.length ? (reportPage - 1) * reportPerPage + 1 : 0
	} to ${Math.min(reportPage * reportPerPage, monthly.length)} of ${monthly.length} months`;

	renderPagination(pages);

	reportElements.statusReport.innerHTML =
		(s.byStatus || [])
			.map(
				(x) =>
					`<div class="d-flex justify-content-between border-bottom py-2"><span>${esc(x.status)}</span><strong>${x.count}</strong></div>`,
			)
			.join("") || '<span class="text-muted">No service data.</span>';

	reportElements.serviceTypeReport.innerHTML =
		(s.byServiceType || [])
			.map(
				(x) =>
					`<div class="d-flex justify-content-between border-bottom py-2"><span>${esc(x.serviceTypeName)}</span><strong>${x.serviceJobCount}</strong></div>`,
			)
			.join("") || '<span class="text-muted">No service type data.</span>';

	reportElements.makeReport.innerHTML =
		(v.byMake || [])
			.map(
				(x) =>
					`<div class="d-flex justify-content-between border-bottom py-2"><span>${esc(x.make)}</span><strong>${x.count}</strong></div>`,
			)
			.join("") || '<span class="text-muted">No data.</span>';

	reportElements.fuelReport.innerHTML =
		(v.byFuelType || [])
			.map(
				(x) =>
					`<div class="d-flex justify-content-between border-bottom py-2"><span>${esc(x.fuelType)}</span><strong>${x.count}</strong></div>`,
			)
			.join("") || '<span class="text-muted">No data.</span>';

	reportElements.vehicleCountReport.innerHTML =
		v.totalVehicles > 0
			? `<div class="d-flex justify-content-between border-bottom py-2"><span>Total Vehicles</span><strong>${v.totalVehicles}</strong></div>
			<div class="d-flex justify-content-between border-bottom py-2"><span>Serviced Vehicles</span><strong>${v.servicedVehicles}</strong></div>
			<div class="d-flex justify-content-between border-bottom py-2"><span>Never Serviced Vehicles</span><strong>${v.neverServicedVehicles}</strong></div>`
			: '<span class="text-muted">No data.</span>';

	reportElements.invActive.textContent = i.totalActiveParts;
	reportElements.invQty.textContent = i.totalQuantity;
	reportElements.invLow.textContent = i.lowStockParts;
	reportElements.invEmpty.textContent = i.outOfStockParts;
	reportElements.invValue.textContent = money(i.inventoryValue);
	reportElements.lowStockReportBody.innerHTML =
		(i.lowStockItems || [])
			.map(
				(x) =>
					`<tr><td>${esc(x.partNumber)}</td><td>${esc(x.name)}</td><td>${x.quantity}</td><td>${x.minimumStock}</td><td>${money(x.unitPrice)}</td><td><span class="badge ${x.isOutOfStock ? "bg-danger" : "bg-warning text-dark"}">${x.isOutOfStock ? "Out of stock" : "Low stock"}</span></td></tr>`,
			)
			.join("") ||
		'<tr><td colspan="6" class="text-center text-muted py-3">No low-stock parts.</td></tr>';
}

function renderPagination(pages) {
	const p = document.getElementById("reportPages");
	p.innerHTML = "";

	if (pages <= 1) return;

	addNavigationButton("Previous", reportPage > 1, () => {
		reportPage--;
		renderReport();
	});

	addPageButton(1);

	const startPage = Math.max(2, reportPage - 1);
	const endPage = Math.min(pages - 1, reportPage + 1);

	if (startPage > 2) addDots();

	for (let i = startPage; i <= endPage; i++) addPageButton(i);

	if (endPage < pages - 1) addDots();

	addPageButton(pages);

	addNavigationButton("Next", reportPage < pages, () => {
		reportPage++;
		renderReport();
	});

	function addPageButton(pageNumber) {
		const li = document.createElement("li");
		li.className = `page-item ${pageNumber === reportPage ? "active" : ""}`;
		li.innerHTML = `<button class="page-link">${pageNumber}</button>`;
		li.querySelector("button").onclick = () => {
			reportPage = pageNumber;
			renderReport();
		};
		p.appendChild(li);
	}

	function addDots() {
		const li = document.createElement("li");
		li.className = "page-item disabled";
		li.innerHTML = '<span class="page-link">...</span>';
		p.appendChild(li);
	}

	function addNavigationButton(text, enabled, action) {
		const li = document.createElement("li");
		li.className = `page-item ${enabled ? "" : "disabled"}`;
		li.innerHTML = `<button class="page-link">${text}</button>`;
		if (enabled) li.querySelector("button").onclick = action;
		p.appendChild(li);
	}
}

function csvCell(value) {
	let text = String(value ?? "");
	// Prevent spreadsheet formula execution for values originating from database fields.
	if (/^[\s]*[=+@\-]/.test(text)) text = "'" + text;
	return `"${text.replaceAll('"', '""')}"`;
}

function exportReports() {
	if (!reportData) {
		reportElements.reportAlert.textContent =
			"Generate a report before exporting.";
		reportElements.reportAlert.className = "alert alert-warning";
		reportElements.reportAlert.classList.remove("d-none");
		return;
	}

	const { revenue: r, services: s, vehicles: v, inventory: i } = reportData;
	const lowStockParts = i.lowStockItems.filter((p) => p.quantity > 0);
	const outOfStockParts = i.lowStockItems.filter((p) => p.quantity == 0);
	const rows = [
		[
			"AutoCare Reports",
			`${reportElements.reportFrom.value} to ${reportElements.reportTo.value}`,
		],
		[],
		[],
		["REVENUE SUMMARY"],
		["Metric", "Value"],
		["Total Invoiced Revenue", r.totalInvoicedRevenue],
		["Total Paid Revenue", r.totalPaidRevenue],
		["Total Unpaid Revenue", r.totalUnpaidRevenue],
		["Total Invoices", r.totalInvoices],
		["Paid Invoices", r.paidInvoices],
		["Unpaid Invoices", r.unpaidInvoices],
		[],
		[],
		["MONTHLY REVENUE"],
		[
			"Month",
			"Invoices",
			"Paid Invoices",
			"Unpaid Invoices",
			"Invoiced Revenue",
			"Paid Revenue",
		],
		...(r.monthlyRevenue || []).map((x) => [
			`${x.monthName.slice(0, 3)} ${x.year}`,
			x.invoiceCount,
			x.paidInvoiceCount,
			x.unpaidInvoiceCount,
			x.invoicedRevenue,
			x.paidRevenue,
		]),
		[],
		[],
		["SERVICE REPORT"],
		["Total Service Jobs", s.totalServiceJobs],
		[],
		["Status", "Count"],
		...(s.byStatus || []).map((x) => [x.status, x.count]),
		[],
		["Service Type", "Service Jobs"],
		...(s.byServiceType || []).map((x) => [
			x.serviceTypeName,
			x.serviceJobCount,
		]),
		[],
		[],
		["VEHICLE REPORT"],
		["Total Vehicles", v.totalVehicles],
		["Serviced Vehicles", v.servicedVehicles],
		["Never Serviced Vehicles", v.neverServicedVehicles],
		[],
		["Make", "Count"],
		...(v.byMake || []).map((x) => [x.make, x.count]),
		[],
		["Fuel Type", "Count"],
		...(v.byFuelType || []).map((x) => [x.fuelType, x.count]),
		[],
		[],
		["INVENTORY REPORT"],
		["Active Parts", i.totalActiveParts],
		["Total Quantity", i.totalQuantity],
		["Low Stock Parts", i.lowStockParts],
		["Out of Stock Parts", i.outOfStockParts],
		["Inventory Value", i.inventoryValue],
		[],
		["Low Stock Parts Count", lowStockParts.length],
		["Part Number", "Part Name", "Quantity", "Minimum Stock", "Unit Price"],
		...lowStockParts.map((x) => [
			x.partNumber,
			x.name,
			x.quantity,
			x.minimumStock,
			x.unitPrice,
		]),
		[],
		["Out of Stock Parts Count", outOfStockParts.length],
		["Part Number", "Part Name", "Quantity", "Minimum Stock", "Unit Price"],
		...outOfStockParts.map((x) => [
			x.partNumber,
			x.name,
			x.quantity,
			x.minimumStock,
			x.unitPrice,
		]),
	];

	const csv = rows.map((row) => row.map(csvCell).join(",")).join("\r\n");
	const blobUrl = URL.createObjectURL(
		new Blob(["\uFEFF", csv], { type: "text/csv;charset=utf-8;" }),
	);
	const a = document.createElement("a");
	a.href = blobUrl;
	a.download = `autocare-reports-${reportElements.reportFrom.value}-${reportElements.reportTo.value}.csv`;
	document.body.appendChild(a);
	a.click();
	a.remove();
	setTimeout(() => URL.revokeObjectURL(blobUrl), 1000);
}
