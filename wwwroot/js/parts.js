let parts = [],
	partEdit = null,
	partPage = 1,
	partModal,
	partViewModal;

const partPerPage = 5;

document.addEventListener("DOMContentLoaded", async () => {
	partModal = new bootstrap.Modal(document.getElementById("partModal"));
	partViewModal = new bootstrap.Modal(
		document.getElementById("partViewModal"),
	);
	if (!isManagement())
		document.getElementById("addPartBtn").classList.add("d-none");

	partSearch.addEventListener("input", () => {
		partPage = 1;
		renderParts();
	});

	stockFilter.addEventListener("change", () => {
		partPage = 1;
		renderParts();
	});

	clearPartFilter.addEventListener("click", () => {
		partSearch.value = "";
		stockFilter.value = "";
		partPage = 1;
		renderParts();
	});

	addPartBtn.addEventListener("click", openPartAdd);
	partForm.addEventListener("submit", savePart);

	await loadParts();
});

async function loadParts() {
	try {
		parts = (await apiGet("/parts")) || [];
		renderParts();

		// Total
		pTotalParts.textContent = parts.length;
		pQty.textContent = parts.reduce((a, x) => a + Number(x.quantity || 0), 0);
		pValue.textContent = money(
			parts.reduce(
				(a, x) => a + Number(x.quantity || 0) * Number(x.unitPrice || 0),
				0,
			),
		);

		// Active
		let activeParts = parts.filter((x) => x.isActive) || [];
		pActiveParts.textContent = activeParts.length;
		pActivePartsQuantity.textContent = activeParts.reduce(
			(a, x) => a + Number(x.quantity || 0),
			0,
		);
		pActiveValue.textContent = money(
			activeParts.reduce(
				(a, x) => a + Number(x.quantity || 0) * Number(x.unitPrice || 0),
				0,
			),
		);

		// Inactive
		let inActiveParts = parts.filter((x) => x.isActive == false) || [];
		pInactiveParts.textContent = inActiveParts.length;
		pInactivePartsQuantity.textContent = inActiveParts.reduce(
			(a, x) => a + Number(x.quantity || 0),
			0,
		);
		pInactiveValue.textContent = money(
			inActiveParts.reduce(
				(a, x) => a + Number(x.quantity || 0) * Number(x.unitPrice || 0),
				0,
			),
		);

		// Low stock and Out of stock
		pLow.textContent = parts.filter(
			(x) => x.isLowStock && x.quantity > 0,
		).length;

		pEmpty.textContent = parts.filter((x) => x.quantity == 0).length;
		
	} catch (e) {
		alertBox("partAlert", e.message, "danger");
	}
}

function renderParts() {
	const q = partSearch.value.toLowerCase().trim();
	const f = stockFilter.value;

	const list = parts.filter(
		(x) =>
			(!q ||
				[x.partNumber, x.name, x.supplier].some((v) =>
					String(v || "")
						.toLowerCase()
						.includes(q),
				)) &&
			(!f ||
				(f === "low" && x.isLowStock && x.quantity > 0) ||
				(f === "out" && x.quantity === 0) ||
				(f === "ok" && !x.isLowStock && x.quantity > 0) ||
				(f === "inactive" && x.isActive == false) ||
				(f === "active" && x.isActive)),
	);

	const pages = Math.max(1, Math.ceil(list.length / partPerPage));
	if (partPage > pages) partPage = pages;

	const page = list.slice(
		(partPage - 1) * partPerPage,
		partPage * partPerPage,
	);

	partBody.innerHTML = page.length
		? page
				.map(
					(x) =>
						`<tr>
							<td><strong>${esc(x.partNumber)}</strong><br>${esc(x.name)}</td>
							<td><strong>${x.quantity}</strong></td>
							<td>${x.minimumStock}</td>
							<td>${money(x.unitPrice)}</td>
							<td>${esc(x.supplier || "-")}</td>
							<td><div>${x.quantity === 0 ? '<span class="badge bg-danger">Out of Stock</span>' : x.isLowStock ? '<span class="badge bg-warning text-dark">Low Stock</span>' : '<span class="badge bg-success">In Stock</span>'}</div><div class="mt-1"><span class="badge ${x.isActive ? "bg-success" : "bg-secondary"}">${x.isActive ? "Active" : "Inactive"}</span></div></td>
							<td>
								<button class="btn btn-sm btn-outline-secondary me-1" title="View part" aria-label="View part" onclick="viewPart(${x.id})"><i class="bi bi-eye"></i></button>
								${isManagement() ? `<button class="btn btn-sm btn-outline-primary me-1" title="Edit part" aria-label="Edit part" onclick="editPart(${x.id})"><i class="bi bi-pencil"></i></button><button class="btn btn-sm btn-outline-danger" title="Delete part" aria-label="Delete part" onclick="deletePart(${x.id})"><i class="bi bi-trash"></i></button>` : ""}
							</td>
						</tr>`,
				)
				.join("")
		: '<tr><td colspan="7" class="text-center text-muted py-4">No parts found.</td></tr>';

	partInfo.textContent = `Showing ${
		list.length ? (partPage - 1) * partPerPage + 1 : 0
	} to ${Math.min(partPage * partPerPage, list.length)} of ${list.length} parts`;

	renderPagination(pages);
}

function renderPagination(pages) {
	const p = document.getElementById("partPages");
	p.innerHTML = "";

	if (pages <= 1) return;

	addNavigationButton("Previous", partPage > 1, () => {
		partPage--;
		renderParts();
	});

	addPageButton(1);

	const startPage = Math.max(2, partPage - 1);
	const endPage = Math.min(pages - 1, partPage + 1);

	if (startPage > 2) addDots();

	for (let i = startPage; i <= endPage; i++) addPageButton(i);

	if (endPage < pages - 1) addDots();

	addPageButton(pages);

	addNavigationButton("Next", partPage < pages, () => {
		partPage++;
		renderParts();
	});

	function addPageButton(pageNumber) {
		const li = document.createElement("li");
		li.className = `page-item ${pageNumber === partPage ? "active" : ""}`;
		li.innerHTML = `<button class="page-link">${pageNumber}</button>`;
		li.querySelector("button").onclick = () => {
			partPage = pageNumber;
			renderParts();
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

function openPartAdd() {
	partEdit = null;
	partForm.reset();
	partTitle.textContent = "Add Part";
	savePartBtn.textContent = "Create Part";
	partActive.value = "true";
	clearPartValidation();
	partFormError.classList.add("d-none");
	partFormError.textContent = "";
	partModal.show();
}

async function viewPart(id) {
	try {
		const x = await apiGet(`/parts/${id}`);
		const fields = {
			viewPartNumber: x.partNumber,
			viewPartName: x.name,
			viewPartDescription: x.description || "-",
			viewPartQuantity: x.quantity,
			viewPartMinimumStock: x.minimumStock,
			viewPartUnitPrice: money(x.unitPrice),
			viewPartSupplier: x.supplier || "-",
			viewPartActive: x.isActive ? "Active" : "Inactive",
			viewPartStockStatus:
				x.quantity === 0
					? "Out of Stock"
					: x.isLowStock
						? "Low Stock"
						: "In Stock",
			viewPartCreatedAt: dateTime(x.createdAt),
			viewPartAssignments: x.serviceJobPartCount ?? 0,
		};
		Object.entries(fields).forEach(
			([id, value]) => (document.getElementById(id).textContent = value),
		);
		partViewModal.show();
	} catch (e) {
		alertBox("partAlert", e.message, "danger");
	}
}

async function editPart(id) {
	try {
		const x = await apiGet(`/parts/${id}`);
		partEdit = id;
		partTitle.textContent = "Edit Part";
		savePartBtn.textContent = "Save Changes";
		partNumber.value = x.partNumber;
		partName.value = x.name;
		partDescription.value = x.description || "";
		partQuantity.value = x.quantity;
		minimumStock.value = x.minimumStock;
		unitPrice.value = x.unitPrice;
		partActive.value = String(x.isActive);
		supplier.value = x.supplier || "";
		clearPartValidation();
		partModal.show();
	} catch (e) {
		alertBox("partAlert", e.message, "danger");
	}
}

function validatePartForm() {
	const errors = [];
	const number = partNumber.value.trim();
	const name = partName.value.trim();
	const description = partDescription.value.trim();
	const quantity = Number(partQuantity.value);
	const minimum = Number(minimumStock.value);
	const price = Number(unitPrice.value);
	const supplierValue = supplier.value.trim();

	if (!number) errors.push("Part Number is required.");
	else if (number.length > 50)
		errors.push("Part Number cannot exceed 50 characters.");

	if (!name) errors.push("Name is required.");
	else if (name.length > 100)
		errors.push("Name cannot exceed 100 characters.");

	if (description.length > 500)
		errors.push("Description cannot exceed 500 characters.");

	if (
		partQuantity.value.trim() === "" ||
		!Number.isSafeInteger(quantity) ||
		quantity < 0
	)
		errors.push("Quantity must be a whole number of 0 or greater.");

	if (
		minimumStock.value.trim() === "" ||
		!Number.isSafeInteger(minimum) ||
		minimum < 0
	)
		errors.push("Minimum Stock must be a whole number of 0 or greater.");

	if (
		unitPrice.value.trim() === "" ||
		!Number.isFinite(price) ||
		price < 0 ||
		price > 9999999999.99
	)
		errors.push("Unit Price must be between 0 and 9,999,999,999.99.");

	if (supplierValue.length > 150)
		errors.push("Supplier cannot exceed 150 characters.");

	if (!["true", "false"].includes(partActive.value))
		errors.push("Select a valid status.");

	partForm.classList.add("was-validated");
	return errors;
}

function clearPartValidation() {
	partForm.classList.remove("was-validated");
	[
		"partNumber",
		"partName",
		"partDescription",
		"partQuantity",
		"minimumStock",
		"unitPrice",
		"partActive",
		"supplier",
	].forEach((id) => {
		document.getElementById(id).classList.remove("is-invalid", "is-valid");
	});
}

async function savePart(e) {
	e.preventDefault();

	const errors = validatePartForm();
	if (errors.length > 0) {
		partFormError.innerHTML = errors
			.map((x) => `<div>${esc(x)}</div>`)
			.join("");
		partFormError.classList.remove("d-none");
		return;
	}

	partFormError.classList.add("d-none");
	partFormError.textContent = "";

	const data = {
		partNumber: partNumber.value.trim(),
		name: partName.value.trim(),
		description: partDescription.value.trim() || null,
		quantity: Number(partQuantity.value),
		minimumStock: Number(minimumStock.value),
		unitPrice: Number(unitPrice.value),
		supplier: supplier.value.trim() || null,
		isActive: partActive.value === "true",
	};

	const btn = savePartBtn;
	if (btn.disabled) return;
	setBusy(btn, true);

	try {
		if (partEdit) await apiPut(`/parts/${partEdit}`, data);
		else await apiPost("/parts", data);

		partModal.hide();
		await loadParts();
		alertBox(
			"partAlert",
			partEdit ? "Part updated successfully." : "Part created successfully.",
		);
	} catch (e) {
		partFormError.textContent = e.message;
		partFormError.classList.remove("d-none");
	} finally {
		setBusy(btn, false);
	}
}

async function deletePart(id) {
	if (!confirm("Delete this part?")) return;

	try {
		await apiDelete(`/parts/${id}`);
		await loadParts();
		alertBox("partAlert", "Part deleted successfully.");
	} catch (e) {
		alertBox("partAlert", e.message, "danger");
	}
}
