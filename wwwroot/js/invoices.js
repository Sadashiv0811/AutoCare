let invoices = [],
	invoiceJobs = [],
	invoiceEdit = null,
	invoicePage = 1,
	invoiceModal;

const MAX_INVOICE_AMOUNT = 99_999_999.99;

const invoicePerPage = 5;

document.addEventListener("DOMContentLoaded", async () => {
	invoiceModal = new bootstrap.Modal("#invoiceModal");

	invoiceSearch.addEventListener("input", () => {
		invoicePage = 1;
		renderInvoices();
	});

	invoiceStatus.addEventListener("change", () => {
		invoicePage = 1;
		renderInvoices();
	});

	clearInvoiceFilter.addEventListener("click", () => {
		invoiceSearch.value = "";
		invoiceStatus.value = "";
		invoicePage = 1;
		renderInvoices();
	});

	addInvoiceBtn.addEventListener("click", openInvoiceAdd);
	invoiceForm.addEventListener("submit", saveInvoice);
	invoicePaid.addEventListener("change", updatePaidAtState);

	if (!isManagement()) {
		addInvoiceBtn.classList.add("d-none");
		invoiceDescription.textContent =
			"View your service invoices and payment status.";
	}

	if (isManagement()) await loadInvoiceJobs();
	await loadInvoices();
});

async function loadInvoiceJobs() {
	try {
		invoiceJobs = (await apiGet("/servicejobs")) || [];

		invoiceJobId.innerHTML =
			'<option value="">Select service job</option>' +
			invoiceJobs
				.filter((x) => !x.hasInvoice)
				.map(
					(x) =>
						`<option value="${x.id}">${esc(x.jobNumber)} — ${esc(x.vehicleRegistrationNumber)} — ${esc(x.serviceTypeName)}</option>`,
				)
				.join("");
	} catch (e) {
		alertBox("invoiceAlert", e.message, "danger");
	}
}

async function loadInvoices() {
	try {
		invoices =
			role() === "Customer"
				? (await apiGet("/invoices/my")) || []
				: (await apiGet("/invoices")) || [];

		renderInvoices();
		iTotal.textContent = invoices.length;
		iPaid.textContent = invoices.filter((x) => x.isPaid).length;
		iUnpaid.textContent = invoices.filter((x) => !x.isPaid).length;
		iValue.textContent = money(
			invoices.reduce((a, x) => a + Number(x.totalAmount || 0), 0),
		);
	} catch (e) {
		alertBox("invoiceAlert", e.message, "danger");
	}
}

function renderInvoices() {
	const q = invoiceSearch.value.toLowerCase().trim();
	const f = invoiceStatus.value;

	const list = invoices.filter(
		(x) =>
			(!f || String(x.isPaid) === f) &&
			(!q ||
				[x.invoiceNumber, x.jobNumber].some((v) =>
					String(v || "")
						.toLowerCase()
						.includes(q),
				)),
	);

	const pages = Math.max(1, Math.ceil(list.length / invoicePerPage));
	if (invoicePage > pages) invoicePage = pages;

	const page = list.slice(
		(invoicePage - 1) * invoicePerPage,
		invoicePage * invoicePerPage,
	);

	invoiceBody.innerHTML = page.length
		? page
				.map(
					(x) =>
						`<tr>
							<td><strong>${esc(x.invoiceNumber)}</strong></td>
							<td>${esc(x.jobNumber)}</td>
							<td>${money(x.serviceCharges)} service<br><small class="text-muted">${money(x.partsAmount)} parts</small></td>
							<td>${money(x.subtotal)}</td>
							<td>${money(x.taxAmount)}</td>
							<td><strong>${money(x.totalAmount)}</strong></td>
							<td>${x.isPaid ? '<span class="badge bg-success">Paid</span>' : '<span class="badge bg-warning text-dark">Unpaid</span>'}</td>
							<td>${dateOnly(x.createdAt)}</td>
							<td>${isManagement() ? `<button class="btn btn-sm btn-outline-primary me-1" onclick="editInvoice(${x.id})" title="Edit invoice"><i class="bi bi-pencil"></i></button>${x.isPaid ? `<button class="btn btn-sm btn-outline-secondary" disabled title="Paid invoices cannot be deleted"><i class="bi bi-trash"></i></button>` : `<button class="btn btn-sm btn-outline-danger" onclick="deleteInvoice(${x.id})" title="Delete unpaid invoice"><i class="bi bi-trash"></i></button>`}` : "-"}</td>
						</tr>`,
				)
				.join("")
		: '<tr><td colspan="9" class="text-center text-muted py-4">No invoices found.</td></tr>';

	invoiceInfo.textContent = `Showing ${
		list.length ? (invoicePage - 1) * invoicePerPage + 1 : 0
	} to ${Math.min(invoicePage * invoicePerPage, list.length)} of ${list.length} invoices`;

	renderPagination(Math.max(1, Math.ceil(list.length / invoicePerPage)));
}

function renderPagination(pages) {
	const p = document.getElementById("invoicePages");
	p.innerHTML = "";

	if (pages <= 1) return;

	addNavigationButton("Previous", invoicePage > 1, () => {
		invoicePage--;
		renderInvoices();
	});

	addPageButton(1);

	const startPage = Math.max(2, invoicePage - 1);
	const endPage = Math.min(pages - 1, invoicePage + 1);

	if (startPage > 2) addDots();

	for (let i = startPage; i <= endPage; i++) addPageButton(i);

	if (endPage < pages - 1) addDots();

	addPageButton(pages);

	addNavigationButton("Next", invoicePage < pages, () => {
		invoicePage++;
		renderInvoices();
	});

	function addPageButton(pageNumber) {
		const li = document.createElement("li");
		li.className = `page-item ${pageNumber === invoicePage ? "active" : ""}`;
		li.innerHTML = `<button class="page-link">${pageNumber}</button>`;
		li.querySelector("button").onclick = () => {
			invoicePage = pageNumber;
			renderInvoices();
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

function openInvoiceAdd() {
	invoiceEdit = null;
	invoiceForm.reset();
	invoiceNumber.value = "";
	invoiceTitle.textContent = "Create Invoice";
	saveInvoiceBtn.textContent = "Create Invoice";
	invoicePaid.value = "false";
	paidAt.value = "";
	clearInvoiceValidation();

	invoiceFormError.classList.add("d-none");
	invoiceFormError.textContent = "";
	invoiceNumberDiv.classList.add("d-none");
	paidAtDiv.classList.add("d-none");

	updatePaidAtState();
	invoiceModal.show();
}

function toInvoiceInput(v) {
	if (!v) return "";
	const d = new Date(v);
	return new Date(d.getTime() - d.getTimezoneOffset() * 60000)
		.toISOString()
		.slice(0, 16);
}

async function editInvoice(id) {
	try {
		const x = await apiGet(`/invoices/${id}`);
		invoiceEdit = id;
		invoiceTitle.textContent = "Edit Invoice";
		saveInvoiceBtn.textContent = "Save Changes";

		invoiceNumberDiv.classList.remove("d-none");
		paidAtDiv.classList.remove("d-none");

		invoiceNumber.value = x.invoiceNumber;
		invoiceJobId.innerHTML = `<option value="${x.serviceJobId}">${esc(x.jobNumber)}</option>`;
		invoiceJobId.value = x.serviceJobId;
		serviceCharges.value = x.serviceCharges;
		taxAmount.value = x.taxAmount;
		invoicePaid.value = String(x.isPaid);
		paidAt.value = toInvoiceInput(x.paidAt);
		clearInvoiceValidation();
		updatePaidAtState();
		invoiceModal.show();
	} catch (e) {
		alertBox("invoiceAlert", e.message, "danger");
	}
}

function validateInvoiceForm() {
	const errors = [];
	const serviceJob = Number(invoiceJobId.value);
	const charges = Number(serviceCharges.value);
	const tax = Number(taxAmount.value);
	const paid = invoicePaid.value === "true";

	if (
		!invoiceJobId.value ||
		!Number.isInteger(serviceJob) ||
		serviceJob <= 0
	) {
		errors.push("Service Job is required.");
	}

	if (
		serviceCharges.value.trim() === "" ||
		!Number.isFinite(charges) ||
		charges < 0
	) {
		errors.push("Service Charges must be a valid amount of 0 or greater.");
	} else if (charges > MAX_INVOICE_AMOUNT) {
		errors.push("Service Charges cannot exceed 99,999,999.99.");
	}

	if (taxAmount.value.trim() === "" || !Number.isFinite(tax) || tax < 0) {
		errors.push("Tax Amount must be a valid amount of 0 or greater.");
	} else if (tax > MAX_INVOICE_AMOUNT) {
		errors.push("Tax Amount cannot exceed 99,999,999.99.");
	}

	if (!["true", "false"].includes(invoicePaid.value)) {
		errors.push("Select a valid payment status.");
	}

	if (paid && paidAt.value && Number.isNaN(new Date(paidAt.value).getTime())) {
		errors.push("Please enter a valid Paid At date and time.");
	}

	invoiceForm.classList.add("was-validated");
	return errors;
}

function clearInvoiceValidation() {
	invoiceForm.classList.remove("was-validated");
	[
		"invoiceNumber",
		"invoiceJobId",
		"serviceCharges",
		"taxAmount",
		"invoicePaid",
		"paidAt",
	].forEach((id) => {
		const el = document.getElementById(id);
		el.classList.remove("is-invalid", "is-valid");
	});
}

function updatePaidAtState() {
	// PaidAt is assigned by the API and cannot be manually forged.
	paidAt.readOnly = true;
	paidAt.disabled = false;
}

function getInvoiceErrorMessage(error) {
	// Handle structured validation errors when available.
	const data = error?.data ?? error?.response?.data;

	if (data?.errors) {
		const messages = Object.values(data.errors).flat().filter(Boolean);
		if (messages.length) return messages.join("\n");
	}

	if (error?.errors && typeof error.errors === "object") {
		const messages = Object.values(error.errors).flat().filter(Boolean);
		if (messages.length) return messages.join("\n");
	}

	if (data?.message) return data.message;

	if (
		data?.title &&
		data.title !== "One or more validation errors occurred."
	) {
		return data.title;
	}

	// Handle an error message containing a serialized JSON response.
	if (error?.message) {
		try {
			const parsed = JSON.parse(error.message);

			if (parsed.errors) {
				const messages = Object.values(parsed.errors)
					.flat()
					.filter(Boolean);
				if (messages.length) return messages.join("\n");
			}

			if (parsed.message) return parsed.message;
			if (
				parsed.title &&
				parsed.title !== "One or more validation errors occurred."
			) {
				return parsed.title;
			}
		} catch {
			// The message is ordinary text, not JSON.
		}

		if (error.message !== "One or more validation errors occurred.") {
			return error.message;
		}
	}

	return "Unable to save the invoice. Please check the entered values and try again.";
}

async function saveInvoice(e) {
	e.preventDefault();

	const errors = validateInvoiceForm();
	if (errors.length > 0) {
		invoiceFormError.innerHTML = errors
			.map((x) => `<div>${esc(x)}</div>`)
			.join("");
		invoiceFormError.classList.remove("d-none");
		return;
	}

	invoiceFormError.classList.add("d-none");
	invoiceFormError.textContent = "";

	const paid = invoicePaid.value === "true";
	const data = {
		serviceJobId: Number(invoiceJobId.value),
		serviceCharges: Number(serviceCharges.value),
		partsAmount: 0,
		taxAmount: Number(taxAmount.value),
		isPaid: paid,
		// The API owns PaidAt; the client does not submit it.
	};

	const btn = saveInvoiceBtn;
	setBusy(btn, true);

	try {
		if (invoiceEdit) await apiPut(`/invoices/${invoiceEdit}`, data);
		else await apiPost("/invoices", data);

		invoiceModal.hide();
		await loadInvoices();
		if (isManagement()) await loadInvoiceJobs();

		alertBox(
			"invoiceAlert",
			invoiceEdit
				? "Invoice updated successfully."
				: "Invoice created successfully.",
		);
	} catch (e) {
		invoiceFormError.textContent = getInvoiceErrorMessage(e);
		invoiceFormError.classList.remove("d-none");
	} finally {
		setBusy(btn, false);
	}
}

async function deleteInvoice(id) {
	const invoice = invoices.find((x) => x.id === id);
	if (invoice?.isPaid) {
		alertBox(
			"invoiceAlert",
			"A paid invoice cannot be deleted. Use an audited correction or refund process instead.",
			"danger",
		);
		return;
	}
	if (!confirm("Delete this invoice?")) return;

	try {
		await apiDelete(`/invoices/${id}`);
		await loadInvoices();
		if (isManagement()) await loadInvoiceJobs();
		alertBox("invoiceAlert", "Invoice deleted successfully.");
	} catch (e) {
		alertBox("invoiceAlert", e.message, "danger");
	}
}
