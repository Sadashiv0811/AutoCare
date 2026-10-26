const statuses = [
	"Scheduled",
	"Received",
	"Inspection",
	"In Progress",
	"Completed",
	"Ready for Pickup",
	"Delivered",
];

let serviceJobs = [],
	serviceEdit = null,
	servicePage = 1,
	serviceModal,
	serviceDetailsModal,
	serviceTypesModal,
	serviceTypesCache = [],
	currentDetailsJob = null;

const servicePerPage = 5;

document.addEventListener("DOMContentLoaded", async () => {
	serviceModal = new bootstrap.Modal("#serviceModal");
	serviceDetailsModal = new bootstrap.Modal("#serviceDetailsModal");
	serviceTypesModal = new bootstrap.Modal("#serviceTypesModal");
	if (isManagement()) manageServiceTypesBtn.classList.remove("d-none");
	manageServiceTypesBtn.addEventListener("click", openServiceTypes);
	serviceTypeForm.addEventListener("submit", saveServiceType);
	cancelServiceTypeEditBtn.addEventListener("click", resetServiceTypeForm);

	serviceSearch.addEventListener("input", () => {
		servicePage = 1;
		renderServiceJobs();
	});

	serviceStatusFilter.addEventListener("change", () => {
		servicePage = 1;
		renderServiceJobs();
	});

	clearServiceFilter.addEventListener("click", () => {
		serviceSearch.value = "";
		serviceStatusFilter.value = "";
		servicePage = 1;
		renderServiceJobs();
	});

	addServiceBtn.addEventListener("click", openServiceAdd);
	serviceForm.addEventListener("submit", saveService);

	if (role() === "Mechanic") {
		addServiceBtn.classList.add("d-none");
		serviceDescription.textContent =
			"View assigned jobs and update their service status.";
	} else if (role() === "Customer") {
		addServiceBtn.classList.add("d-none");
		serviceDescription.textContent =
			"View your service jobs and current service status.";
	}

	await loadServiceLookups();
	await loadServiceJobs();
});

async function loadServiceLookups() {
	if (!isManagement()) return;

	try {
		const [vehicles, mechanics, types] = await Promise.all([
			apiGet("/vehicles"),
			apiGet("/mechanics"),
			apiGet("/servicetypes"),
		]);

		vehicleId.innerHTML =
			'<option value="">Select vehicle</option>' +
			(vehicles || [])
				.map(
					(x) =>
						`<option value="${x.id}">${esc(x.registrationNumber)} — ${esc(x.make)} ${esc(x.model)}</option>`,
				)
				.join("");

		mechanicId.innerHTML =
			'<option value="">Select mechanic</option>' +
			(mechanics || [])
				.filter((x) => x.isActive)
				.map((x) => `<option value="${x.id}">${esc(x.fullName)}</option>`)
				.join("");

		serviceTypeId.innerHTML =
			'<option value="">Select service type</option>' +
			(types || [])
				.map((x) => `<option value="${x.id}">${esc(x.name)}</option>`)
				.join("");
	} catch (e) {
		alertBox("serviceAlert", e.message, "danger");
	}
}

async function loadServiceJobs() {
	try {
		serviceJobs =
			role() === "Customer"
				? (await apiGet("/servicejobs/my")) || []
				: role() === "Mechanic"
					? (await apiGet("/servicejobs/assigned")) || []
					: (await apiGet("/servicejobs")) || [];

		renderServiceJobs();

		sTotal.textContent = serviceJobs.length;

		sScheduled.textContent = serviceJobs.filter((x) => x.status == 0).length;

		sReceived.textContent = serviceJobs.filter((x) => x.status == 1).length;

		sInspection.textContent = serviceJobs.filter((x) => x.status == 2).length;

		sProgress.textContent = serviceJobs.filter((x) => x.status == 3).length;

		sCompleted.textContent = serviceJobs.filter((x) => x.status == 4).length;

		sReadyForPickup.textContent = serviceJobs.filter((x) => x.status == 5).length;

		sDelivered.textContent = serviceJobs.filter((x) => x.status == 6).length;

		sInvoice.textContent = serviceJobs.filter((x) => x.hasInvoice).length;
	} catch (e) {
		alertBox("serviceAlert", e.message, "danger");
	}
}

function renderServiceJobs() {
	const q = serviceSearch.value.toLowerCase().trim();
	const f = serviceStatusFilter.value;

	const list = serviceJobs.filter(
		(x) =>
			(!q ||
				[
					x.jobNumber,
					x.vehicleRegistrationNumber,
					x.mechanicName,
					x.serviceTypeName,
				].some((v) =>
					String(v || "")
						.toLowerCase()
						.includes(q),
				)) &&
			(!f || String(x.status) === f),
	);

	const pages = Math.max(1, Math.ceil(list.length / servicePerPage));
	if (servicePage > pages) servicePage = pages;

	const page = list.slice(
		(servicePage - 1) * servicePerPage,
		servicePage * servicePerPage,
	);

	serviceBody.innerHTML = page.length
		? page
				.map(
					(x) => `
						<tr>
							<td><strong>${esc(x.jobNumber)}</strong></td>
							<td>${esc(x.vehicleRegistrationNumber)}</td>
							<td>${esc(x.mechanicName)}</td>
							<td>${esc(x.serviceTypeName)}</td>
							<td>${dateTime(x.scheduledDate)}</td>
							<td><span class="badge ${x.status >= 4 ? "bg-success" : "bg-primary"}">${esc(statuses[x.status] || "Unknown")}</span></td>
							<td>${x.partCount || 0}</td>
							<td>${x.hasInvoice ? '<span class="badge bg-success">Yes</span>' : '<span class="badge bg-secondary">No</span>'}</td>
							<td>
								<button class="btn btn-sm btn-outline-info me-1" onclick="viewService(${x.id})">
									<i class="bi bi-eye"></i>
								</button>
								${
									isManagement()
										? `<button class="btn btn-sm btn-outline-primary me-1" onclick="editService(${x.id})"><i class="bi bi-pencil"></i></button>
										   <button class="btn btn-sm btn-outline-danger" onclick="deleteService(${x.id})"><i class="bi bi-trash"></i></button>`
										: role() === "Mechanic"
											? `<button class="btn btn-sm btn-outline-warning" onclick="updateJobStatus(${x.id}, ${x.status})"><i class="bi bi-arrow-repeat"></i></button>`
											: ""
								}
							</td>
						</tr>
					`,
				)
				.join("")
		: '<tr><td colspan="9" class="text-center text-muted py-4">No service jobs found.</td></tr>';

	serviceInfo.textContent = `Showing ${
		list.length ? (servicePage - 1) * servicePerPage + 1 : 0
	} to ${Math.min(servicePage * servicePerPage, list.length)} of ${list.length} service jobs`;

	renderPagination(pages);
}

function renderPagination(pages) {
	const p = document.getElementById("servicePages");
	p.innerHTML = "";

	if (pages <= 1) return;

	addNavigationButton("Previous", servicePage > 1, () => {
		servicePage--;
		renderServiceJobs();
	});

	addPageButton(1);

	const startPage = Math.max(2, servicePage - 1);
	const endPage = Math.min(pages - 1, servicePage + 1);

	if (startPage > 2) addDots();

	for (let i = startPage; i <= endPage; i++) addPageButton(i);

	if (endPage < pages - 1) addDots();

	addPageButton(pages);

	addNavigationButton("Next", servicePage < pages, () => {
		servicePage++;
		renderServiceJobs();
	});

	function addPageButton(pageNumber) {
		const li = document.createElement("li");
		li.className = `page-item ${pageNumber === servicePage ? "active" : ""}`;
		li.innerHTML = `<button class="page-link">${pageNumber}</button>`;
		li.querySelector("button").onclick = () => {
			servicePage = pageNumber;
			renderServiceJobs();
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

function toLocalInput(v) {
	const d = new Date(v);
	return new Date(d.getTime() - d.getTimezoneOffset() * 60000)
		.toISOString()
		.slice(0, 16);
}

function openServiceAdd() {
	serviceEdit = null;
	serviceForm.reset();
	serviceModalTitle.textContent = "Create Service Job";
	saveServiceBtn.textContent = "Create Job";
	jobStatus.value = "0";
	clearServiceValidation();
	serviceFormError.classList.add("d-none");
	serviceFormError.textContent = "";
	serviceModal.show();
}

async function editService(id) {
	try {
		const x = await apiGet(`/servicejobs/${id}`);
		serviceEdit = id;
		serviceModalTitle.textContent = "Edit Service Job";
		saveServiceBtn.textContent = "Save Changes";

		vehicleId.value = x.vehicleId;
		mechanicId.value = x.mechanicId;
		serviceTypeId.value = x.serviceTypeId;
		jobMileage.value = x.mileage;
		jobStatus.value = x.status;
		scheduledDate.value = toLocalInput(x.scheduledDate);
		problemDescription.value = x.problemDescription || "";
		inspectionNotes.value = x.inspectionNotes || "";
		clearServiceValidation();
		serviceModal.show();
	} catch (e) {
		alertBox("serviceAlert", e.message, "danger");
	}
}

function validateServiceForm() {
	const errors = [];
	const mileage = Number(jobMileage.value);
	const scheduled = scheduledDate.value;
	const status = Number(jobStatus.value);
	const problem = problemDescription.value.trim();
	const notes = inspectionNotes.value.trim();

	if (!vehicleId.value) errors.push("Vehicle is required.");
	if (!mechanicId.value) errors.push("Mechanic is required.");
	if (!serviceTypeId.value) errors.push("Service Type is required.");

	if (!Number.isFinite(mileage) || mileage < 0)
		errors.push("Mileage must be 0 or greater.");

	if (!scheduled) {
		errors.push("Scheduled Date is required.");
	} else if (Number.isNaN(new Date(scheduled).getTime())) {
		errors.push("Please enter a valid Scheduled Date.");
	}

	if (!Number.isInteger(status) || status < 0 || status > 6)
		errors.push("Select a valid service status.");

	if (problem.length > 1000)
		errors.push("Problem Description cannot exceed 1000 characters.");

	if (notes.length > 2000)
		errors.push("Inspection Notes cannot exceed 2000 characters.");

	serviceForm.classList.add("was-validated");
	return errors;
}

function clearServiceValidation() {
	serviceForm.classList.remove("was-validated");
	[
		"scheduledDate",
		"vehicleId",
		"mechanicId",
		"serviceTypeId",
		"jobMileage",
		"jobStatus",
		"problemDescription",
		"inspectionNotes",
	].forEach((id) => {
		const el = document.getElementById(id);
		el.classList.remove("is-invalid", "is-valid");
	});
}

async function saveService(e) {
	e.preventDefault();

	const errors = validateServiceForm();
	if (errors.length > 0) {
		serviceFormError.innerHTML = errors
			.map((x) => `<div>${esc(x)}</div>`)
			.join("");
		serviceFormError.classList.remove("d-none");
		return;
	}

	serviceFormError.classList.add("d-none");
	serviceFormError.textContent = "";

	const btn = saveServiceBtn;
	setBusy(btn, true);

	try {
		const data = {
			vehicleId: Number(vehicleId.value),
			mechanicId: Number(mechanicId.value),
			serviceTypeId: Number(serviceTypeId.value),
			mileage: Number(jobMileage.value),
			problemDescription: problemDescription.value.trim() || null,
			inspectionNotes: inspectionNotes.value.trim() || null,
			status: serviceEdit ? Number(jobStatus.value) : 0,
			scheduledDate: `${scheduledDate.value}:00`,
		};

		if (serviceEdit) {
			await apiPut(`/servicejobs/${serviceEdit}`, data);
		} else {
			await apiPost("/servicejobs", data);
		}

		serviceModal.hide();
		await loadServiceJobs();
		alertBox(
			"serviceAlert",
			serviceEdit
				? "Service job updated successfully."
				: "Service job created successfully.",
		);
	} catch (e) {
		serviceFormError.textContent = e.message;
		serviceFormError.classList.remove("d-none");
	} finally {
		setBusy(btn, false);
	}
}

async function deleteService(id) {
	if (
		!confirm(
			"Delete this service job? Used parts will be restored by the API.",
		)
	)
		return;

	try {
		await apiDelete(`/servicejobs/${id}`);
		await loadServiceJobs();
		alertBox("serviceAlert", "Service job deleted successfully.");
	} catch (e) {
		alertBox("serviceAlert", e.message, "danger");
	}
}

async function updateJobStatus(id, current) {
	const next = prompt(
		`Current status: ${statuses[current]}\nEnter new status number:\n0 Scheduled, 1 Received, 2 Inspection, 3 In Progress, 4 Completed, 5 Ready for Pickup, 6 Delivered`,
		current,
	);

	if (next === null) return;

	const nextNumber = Number(next);
	if (!Number.isInteger(nextNumber) || nextNumber < 0 || nextNumber > 6) {
		alertBox(
			"serviceAlert",
			"Please enter a valid status number from 0 to 6.",
			"warning",
		);
		return;
	}

	try {
		await apiRequest(`/servicejobs/${id}/status`, {
			method: "PATCH",
			body: JSON.stringify({ status: nextNumber }),
		});
		await loadServiceJobs();
		alertBox("serviceAlert", "Service status updated.");
	} catch (e) {
		alertBox("serviceAlert", e.message, "danger");
	}
}

async function viewService(id) {
	try {
		// Each role must use its own scoped endpoint. The generic /servicejobs/{id}
		// endpoint is restricted to Admin and Service Advisor.
		const detailsEndpoint =
			role() === "Customer"
				? `/servicejobs/my/${id}`
				: role() === "Mechanic"
					? `/servicejobs/assigned/${id}`
					: `/servicejobs/${id}`;

		const x = await apiGet(detailsEndpoint);
		currentDetailsJob = x;

		// Parts/notes management is reserved for management roles. Customer and
		// Mechanic views do not call the management collection endpoints.
		const canManagePartsNotes = ["Admin", "Service Advisor"].includes(role());
		const [allParts, allNotes] = canManagePartsNotes
			? await Promise.all([
					apiGet("/servicejobparts"),
					apiGet("/servicenotes"),
				])
			: [[], []];
		const jobParts = (allParts || []).filter((p) => p.serviceJobId === id);
		const jobNotes = (allNotes || []).filter((n) => n.serviceJobId === id);
		const parts = canManagePartsNotes
			? ((await apiGet("/parts")) || []).filter((p) => p.isActive)
			: [];
		const mechanics = canManagePartsNotes
			? ((await apiGet("/mechanics")) || []).filter((m) => m.isActive)
			: [];
		const editable = canManagePartsNotes;
		serviceDetailsBody.innerHTML = `<div class="row g-3 mb-4">
			<div class="col-md-4"><strong>Job Number</strong><div>${esc(x.jobNumber)}</div></div>
			<div class="col-md-4"><strong>Status</strong><div>${esc(statuses[x.status] || "Unknown")}</div></div>
			<div class="col-md-4"><strong>Scheduled</strong><div>${dateTime(x.scheduledDate)}</div></div>
			<div class="col-md-4"><strong>Vehicle</strong><div>${esc(x.vehicleRegistrationNumber)}</div></div>
			<div class="col-md-4"><strong>Mechanic</strong><div>${esc(x.mechanicName)}</div></div>
			<div class="col-md-4"><strong>Service</strong><div>${esc(x.serviceTypeName)}</div></div>
			<div class="col-md-4"><strong>Mileage</strong><div>${Number(x.mileage).toLocaleString("en-IN")} km</div></div>
			<div class="col-md-4"><strong>Invoice</strong><div>${x.hasInvoice ? "Created" : "Not created"}</div></div>
			<div class="col-12"><strong>Problem</strong><div>${esc(x.problemDescription || "-")}</div></div>
			<div class="col-12"><strong>Inspection Notes</strong><div>${esc(x.inspectionNotes || "-")}</div></div>
		</div>
		<hr><div class="d-flex justify-content-between align-items-center mb-2"><h6 class="mb-0">Parts Used</h6>${editable ? '<button class="btn btn-sm btn-primary" onclick="openJobPartForm()"><i class="bi bi-plus-lg me-1"></i>Add Part</button>' : ""}</div>
		<div id="jobPartFormHost"></div><div class="table-responsive mb-4"><table class="table table-sm align-middle"><thead><tr><th>Part</th><th>Qty</th><th>Unit Price</th><th>Total</th>${editable ? "<th>Actions</th>" : ""}</tr></thead><tbody>${jobParts.length ? jobParts.map((p) => `<tr><td>${esc(p.partNumber)} — ${esc(p.partName)}</td><td>${p.quantity}</td><td>₹${Number(p.unitPrice).toFixed(2)}</td><td>₹${Number(p.totalPrice).toFixed(2)}</td>${editable ? `<td><button class="btn btn-sm btn-outline-primary me-1" onclick="editJobPart(${p.id})"><i class="bi bi-pencil"></i></button><button class="btn btn-sm btn-outline-danger" onclick="deleteJobPart(${p.id})"><i class="bi bi-trash"></i></button></td>` : ""}</tr>`).join("") : `<tr><td colspan="${editable ? 5 : 4}" class="text-muted text-center">No parts recorded for this job.</td></tr>`}</tbody></table></div>
		<hr><div class="d-flex justify-content-between align-items-center mb-2"><h6 class="mb-0">Service Notes</h6>${editable ? '<button class="btn btn-sm btn-primary" onclick="openJobNoteForm()"><i class="bi bi-plus-lg me-1"></i>Add Note</button>' : ""}</div>
		<div id="jobNoteFormHost"></div><div id="jobNotesList">${renderJobNotes(jobNotes, editable)}</div>`;
		window.currentJobParts = jobParts;
		window.currentJobNotes = jobNotes;
		window.currentJobPartOptions = parts;
		window.currentJobMechanicOptions = mechanics;
		serviceDetailsModal.show();
	} catch (e) {
		alertBox("serviceAlert", e.message, "danger");
	}
}

function renderJobNotes(notes, editable) {
	if (!notes.length)
		return '<p class="text-muted">No service notes recorded for this job.</p>';
	return notes
		.map(
			(n) =>
				`<div class="border rounded p-3 mb-2"><div class="d-flex justify-content-between gap-2"><div><div class="fw-semibold">${esc(n.mechanicName || "Unassigned mechanic")}</div><small class="text-muted">${dateTime(n.createdAt)}</small></div>${editable ? `<div class="text-nowrap"><button class="btn btn-sm btn-outline-primary me-1" onclick="editJobNote(${n.id})"><i class="bi bi-pencil"></i></button><button class="btn btn-sm btn-outline-danger" onclick="deleteJobNote(${n.id})"><i class="bi bi-trash"></i></button></div>` : ""}</div><div class="mt-2">${esc(n.note)}</div></div>`,
		)
		.join("");
}

function openJobPartForm(part = null) {
	if (!currentDetailsJob) return;
	const options = window.currentJobPartOptions || [];
	const existing = window.currentJobParts || [];
	jobPartEditId = part ? part.id : null;
	document.getElementById("jobPartFormHost").innerHTML =
		`<form id="jobPartInlineForm" class="border rounded p-3 mb-3"><div class="row g-2 align-items-end"><div class="col-md-6"><label class="form-label">Part *</label><select id="jobPartPartId" class="form-select" required><option value="">Select part</option>${options.map((p) => `<option value="${p.id}" ${part && part.partId === p.id ? "selected" : ""}>${esc(p.partNumber)} — ${esc(p.name)} (stock: ${p.quantity})</option>`).join("")}</select></div><div class="col-md-3"><label class="form-label">Quantity *</label><input id="jobPartQuantity" class="form-control" type="number" min="1" step="1" required value="${part ? part.quantity : 1}"></div><div class="col-md-3 d-flex gap-2"><button class="btn btn-primary" type="submit">${part ? "Save" : "Add Part"}</button><button class="btn btn-outline-secondary" type="button" onclick="document.getElementById('jobPartFormHost').innerHTML=''"><i class="bi bi-x-lg"></i></button></div></div><div id="jobPartError" class="text-danger small mt-2"></div></form>`;
	document
		.getElementById("jobPartInlineForm")
		.addEventListener("submit", saveJobPart);
}
let jobPartEditId = null;
async function editJobPart(id) {
	const p = (window.currentJobParts || []).find((x) => x.id === id);
	if (p) openJobPartForm(p);
}

async function saveJobPart(e) {
	e.preventDefault();

	const partId = Number(jobPartPartId.value);
	const quantity = Number(jobPartQuantity.value);

	const part = (window.currentJobPartOptions || []).find(
		(p) => p.id === partId,
	);

	if (!partId || !Number.isInteger(quantity) || quantity < 1) {
		jobPartError.textContent =
			"Select a part and enter a positive whole-number quantity.";
		return;
	}

	const existingLine = jobPartEditId
		? (window.currentJobParts || []).find((p) => p.id === jobPartEditId)
		: null;

	const available =
		part.quantity +
		(existingLine && existingLine.partId === partId
			? existingLine.quantity
			: 0);

	if (quantity > available) {
		jobPartError.textContent = `Insufficient stock. Available quantity: ${available}.`;
		return;
	}

	try {
		const data = {
			serviceJobId: currentDetailsJob.id,
			partId,
			quantity,
		};

		if (jobPartEditId) {
			await apiPut(`/servicejobparts/${jobPartEditId}`, data);
		} else {
			await apiPost("/servicejobparts", data);
		}

		await viewService(currentDetailsJob.id);
		await loadServiceJobs();

		alertBox("serviceAlert", "Parts used updated successfully.");
	} catch (err) {
		jobPartError.textContent = err.message;
	}
}

async function deleteJobPart(id) {
	if (
		!confirm(
			"Remove this part from the service job? The API will restore the stock quantity.",
		)
	) {
		return;
	}

	try {
		await apiDelete(`/servicejobparts/${id}`);

		await viewService(currentDetailsJob.id);
		await loadServiceJobs();

		alertBox("serviceAlert", "Part removed and stock restored.");
	} catch (e) {
		alertBox("serviceAlert", e.message, "danger");
	}
}

function openJobNoteForm(note = null) {
	if (!currentDetailsJob) return;
	jobNoteEditId = note ? note.id : null;
	const mechanics = window.currentJobMechanicOptions || [];
	document.getElementById("jobNoteFormHost").innerHTML =
		`<form id="jobNoteInlineForm" class="border rounded p-3 mb-3"><div class="mb-2"><label class="form-label">Mechanic</label><select id="jobNoteMechanicId" class="form-select"><option value="">Use assigned mechanic (${esc(currentDetailsJob.mechanicName || "none")})</option>${mechanics.map((m) => `<option value="${m.id}" ${note && note.mechanicId === m.id ? "selected" : !note && m.id === currentDetailsJob.mechanicId ? "selected" : ""}>${esc(m.fullName)}</option>`).join("")}</select></div><div class="mb-2"><label class="form-label">Note *</label><textarea id="jobNoteText" class="form-control" rows="3" maxlength="2000" required>${note ? esc(note.note) : ""}</textarea></div><div id="jobNoteError" class="text-danger small mb-2"></div><button class="btn btn-primary" type="submit">${note ? "Save Note" : "Add Note"}</button> <button class="btn btn-outline-secondary" type="button" onclick="document.getElementById('jobNoteFormHost').innerHTML='';jobNoteEditId=null">Cancel</button></form>`;
	document
		.getElementById("jobNoteInlineForm")
		.addEventListener("submit", saveJobNote);
}
let jobNoteEditId = null;
async function editJobNote(id) {
	const n = (window.currentJobNotes || []).find((x) => x.id === id);
	if (n) openJobNoteForm(n);
}
async function saveJobNote(e) {
	e.preventDefault();
	const note = jobNoteText.value.trim();
	if (!note || note.length > 2000) {
		jobNoteError.textContent =
			"Note is required and must be no longer than 2000 characters.";
		return;
	}
	const rawMechanic = jobNoteMechanicId.value;
	const data = {
		serviceJobId: currentDetailsJob.id,
		mechanicId: rawMechanic
			? Number(rawMechanic)
			: currentDetailsJob.mechanicId || null,
		note,
	};
	try {
		if (jobNoteEditId) await apiPut(`/servicenotes/${jobNoteEditId}`, data);
		else await apiPost("/servicenotes", data);
		await viewService(currentDetailsJob.id);
		alertBox("serviceAlert", "Service note saved successfully.");
	} catch (err) {
		jobNoteError.textContent = err.message;
	}
}
async function deleteJobNote(id) {
	if (!confirm("Delete this service note?")) return;
	try {
		await apiDelete(`/servicenotes/${id}`);
		await viewService(currentDetailsJob.id);
		alertBox("serviceAlert", "Service note deleted.");
	} catch (e) {
		alertBox("serviceAlert", e.message, "danger");
	}
}

async function openServiceTypes() {
	if (!isManagement()) return;
	resetServiceTypeForm();
	serviceTypesModal.show();
	await loadServiceTypes();
}
async function loadServiceTypes() {
	try {
		serviceTypesCache = (await apiGet("/servicetypes")) || [];
		serviceTypesBody.innerHTML = serviceTypesCache.length
			? serviceTypesCache
					.map(
						(t) =>
							`<tr><td><div class="fw-semibold">${esc(t.name)}</div><small class="text-muted">${esc(t.description || "")}</small></td><td>${t.estimatedDurationMinutes} min</td><td>₹${Number(t.basePrice).toFixed(2)}</td><td>${t.serviceJobCount || 0}</td><td><span class="badge ${t.isActive ? "bg-success" : "bg-secondary"}">${t.isActive ? "Active" : "Inactive"}</span></td><td class="text-nowrap"><button class="btn btn-sm btn-outline-primary me-1" onclick="editServiceType(${t.id})"><i class="bi bi-pencil"></i></button><button class="btn btn-sm btn-outline-danger" onclick="deleteServiceType(${t.id})"><i class="bi bi-trash"></i></button></td></tr>`,
					)
					.join("")
			: '<tr><td colspan="6" class="text-center text-muted">No service types found.</td></tr>';
	} catch (e) {
		alertBox("serviceTypesAlert", e.message, "danger");
	}
}
function resetServiceTypeForm() {
	serviceTypeForm.reset();
	serviceTypeEditId.value = "";
	typeIsActive.checked = true;
	serviceTypeForm.classList.remove("was-validated");
	serviceTypeFormTitle.textContent = "Add Service Type";
	saveServiceTypeBtn.textContent = "Add Service Type";
	cancelServiceTypeEditBtn.classList.add("d-none");
}
function editServiceType(id) {
	const t = serviceTypesCache.find((x) => x.id === id);
	if (!t) return;
	serviceTypeEditId.value = t.id;
	typeName.value = t.name;
	typeDuration.value = t.estimatedDurationMinutes;
	typePrice.value = t.basePrice;
	typeDescription.value = t.description || "";
	typeIsActive.checked = t.isActive;
	serviceTypeFormTitle.textContent = "Edit Service Type";
	saveServiceTypeBtn.textContent = "Save Changes";
	cancelServiceTypeEditBtn.classList.remove("d-none");
}
async function saveServiceType(e) {
	e.preventDefault();
	const name = typeName.value.trim(),
		duration = Number(typeDuration.value),
		price = Number(typePrice.value);
	serviceTypeForm.classList.add("was-validated");
	if (
		!name ||
		name.length > 100 ||
		!Number.isInteger(duration) ||
		duration < 1 ||
		!Number.isFinite(price) ||
		price < 0 ||
		typeDescription.value.length > 500
	) {
		alertBox(
			"serviceTypesAlert",
			"Check the service name, duration, price, and description.",
			"danger",
		);
		return;
	}
	const data = {
		name,
		description: typeDescription.value.trim() || null,
		estimatedDurationMinutes: duration,
		basePrice: price,
		isActive: typeIsActive.checked,
	};
	const id = Number(serviceTypeEditId.value);
	try {
		if (id) await apiPut(`/servicetypes/${id}`, data);
		else await apiPost("/servicetypes", data);
		resetServiceTypeForm();
		await loadServiceTypes();
		await loadServiceLookups();
		alertBox(
			"serviceTypesAlert",
			id ? "Service type updated." : "Service type created.",
		);
	} catch (err) {
		alertBox("serviceTypesAlert", err.message, "danger");
	}
}
async function deleteServiceType(id) {
	const t = serviceTypesCache.find((x) => x.id === id);
	if (!t) return;
	if ((t.serviceJobCount || 0) > 0) {
		alertBox(
			"serviceTypesAlert",
			"This service type is used by existing jobs. Edit it and mark it inactive instead of deleting it.",
			"warning",
		);
		return;
	}
	if (!confirm(`Delete service type "${t.name}"?`)) return;
	try {
		await apiDelete(`/servicetypes/${id}`);
		await loadServiceTypes();
		await loadServiceLookups();
		alertBox("serviceTypesAlert", "Service type deleted.");
	} catch (e) {
		alertBox("serviceTypesAlert", e.message, "danger");
	}
}
