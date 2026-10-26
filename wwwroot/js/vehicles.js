let vehicles = [],
	customers = [],
	vehicleEditingId = null,
	vehiclePage = 1;

const vehiclePerPage = 5;

let vehicleModal, historyModal;

document.addEventListener("DOMContentLoaded", initVehicles);

async function initVehicles() {
	vehicleModal = new bootstrap.Modal("#vehicleModal");
	historyModal = new bootstrap.Modal("#historyModal");
	document.getElementById("vehicleSearch").addEventListener("input", () => {
		vehiclePage = 1;
		renderVehicles();
	});
	document.getElementById("fuelFilter").addEventListener("change", () => {
		vehiclePage = 1;
		renderVehicles();
	});
	document
		.getElementById("clearVehicleFilter")
		.addEventListener("click", () => {
			document.getElementById("vehicleSearch").value = "";
			document.getElementById("fuelFilter").value = "";
			vehiclePage = 1;
			renderVehicles();
		});
	document
		.getElementById("addVehicleBtn")
		.addEventListener("click", openVehicleAdd);
	document
		.getElementById("vehicleForm")
		.addEventListener("submit", saveVehicle);
	if (role() === "Customer") {
		document.getElementById("addVehicleBtn").classList.add("d-none");
		document.getElementById("pageDescription").textContent =
			"View your vehicles and service history.";
	} else await loadCustomers();
	await loadVehicles();
}

async function loadCustomers() {
	try {
		customers = (await apiGet("/customers")) || [];
		document.getElementById("customerId").innerHTML =
			'<option value="">Select customer</option>' +
			customers
				.map(
					(c) =>
						`<option value="${c.id}">${esc(c.fullName)} — ${esc(c.phone)}</option>`,
				)
				.join("");
	} catch (e) {
		alertBox("vehicleAlert", e.message, "danger");
	}
}

async function loadVehicles() {
	try {
		vehicles =
			role() === "Customer"
				? await apiGet("/vehicles/my")
				: await apiGet("/vehicles");
		vehicles = vehicles || [];
		renderVehicles();
		updateVehicleSummary();
	} catch (e) {
		alertBox("vehicleAlert", e.message, "danger");
	}
}

function filteredVehicles() {
	const q = document
			.getElementById("vehicleSearch")
			.value.toLowerCase()
			.trim(),
		fuel = document.getElementById("fuelFilter").value;
	return vehicles
		.filter((v) => !fuel || v.fuelType === fuel)
		.filter(
			(v) =>
				!q ||
				[v.registrationNumber, v.vin, v.make, v.model, v.customerName].some(
					(x) =>
						String(x || "")
							.toLowerCase()
							.includes(q),
				),
		);
}

function renderVehicles() {
	const list = filteredVehicles(),
		pages = Math.max(1, Math.ceil(list.length / vehiclePerPage));
	if (vehiclePage > pages) vehiclePage = pages;
	const page = list.slice(
			(vehiclePage - 1) * vehiclePerPage,
			vehiclePage * vehiclePerPage,
		),
		body = document.getElementById("vehicleBody");
	body.innerHTML = page.length
		? page
				.map(
					(
						v,
					) => `<tr><td>${v.id}</td><td><strong>${esc(v.registrationNumber)}</strong><br><small class="text-muted">${esc(v.vin)}</small></td><td>${esc(v.make)} ${esc(v.model)}<br><small>${v.year} · ${esc(v.color || "-")}</small></td><td>${esc(v.customerName || "-")}</td><td>${esc(v.fuelType)}</td><td>${Number(v.mileage).toLocaleString("en-IN")} km</td><td>
 <button class="btn btn-sm btn-outline-info me-1" onclick="viewVehicle(${v.id})" title="History"><i class="bi bi-clock-history"></i></button>
 ${role() !== "Customer" ? `<button class="btn btn-sm btn-outline-primary me-1" onclick="editVehicle(${v.id})"><i class="bi bi-pencil"></i></button><button class="btn btn-sm btn-outline-danger" onclick="deleteVehicle(${v.id})"><i class="bi bi-trash"></i></button>` : ""}</td></tr>`,
				)
				.join("")
		: `<tr><td colspan="7" class="text-center text-muted py-4">No vehicles found.</td></tr>`;
	document.getElementById("vehiclePageInfo").textContent =
		`Showing ${list.length ? (vehiclePage - 1) * vehiclePerPage + 1 : 0} to ${Math.min(vehiclePage * vehiclePerPage, list.length)} of ${list.length} vehicles`;
	renderPagination(pages);
}

function renderPagination(pages) {
	const p = document.getElementById("vehiclePagination");
	p.innerHTML = "";

	if (pages <= 1) return;

	// Previous
	addNavigationButton("Previous", vehiclePage > 1, () => {
		vehiclePage--;
		renderVehicles();
	});

	// Always show page 1
	addPageButton(1);

	// Pages around current page
	const startPage = Math.max(2, vehiclePage - 1);
	const endPage = Math.min(pages - 1, vehiclePage + 1);

	// Left dots
	if (startPage > 2) {
		addDots();
	}

	// Current page and nearby pages
	for (let i = startPage; i <= endPage; i++) {
		addPageButton(i);
	}

	// Right dots
	if (endPage < pages - 1) {
		addDots();
	}

	// Always show last page
	if (pages > 1) {
		addPageButton(pages);
	}

	// Next
	addNavigationButton("Next", vehiclePage < pages, () => {
		vehiclePage++;
		renderVehicles();
	});

	function addPageButton(pageNumber) {
		const li = document.createElement("li");

		li.className = `page-item ${pageNumber === vehiclePage ? "active" : ""}`;

		li.innerHTML = `
			<button class="page-link">${pageNumber}</button>
		`;

		li.querySelector("button").onclick = () => {
			vehiclePage = pageNumber;
			renderVehicles();
		};

		p.appendChild(li);
	}

	function addDots() {
		const li = document.createElement("li");

		li.className = "page-item disabled";

		li.innerHTML = `
			<span class="page-link">...</span>
		`;

		p.appendChild(li);
	}

	function addNavigationButton(text, enabled, action) {
		const li = document.createElement("li");

		li.className = `page-item ${enabled ? "" : "disabled"}`;

		li.innerHTML = `
			<button class="page-link">${text}</button>
		`;

		if (enabled) {
			li.querySelector("button").onclick = action;
		}

		p.appendChild(li);
	}
}

function updateVehicleSummary() {
	document.getElementById("totalVehicles").textContent = vehicles.length;
	document.getElementById("totalMakes").textContent = new Set(
		vehicles.map((v) => v.make),
	).size;
	document.getElementById("dieselCount").textContent = vehicles.filter(
		(v) => v.fuelType === "Diesel",
	).length;
	document.getElementById("petrolCount").textContent = vehicles.filter(
		(v) => v.fuelType === "Petrol",
	).length;
	document.getElementById("electricCount").textContent = vehicles.filter(
		(v) => v.fuelType === "Electric",
	).length;
	document.getElementById("hybridCount").textContent = vehicles.filter(
		(v) => v.fuelType === "Hybrid",
	).length;
	document.getElementById("cngCount").textContent = vehicles.filter(
		(v) => v.fuelType === "CNG",
	).length;
}

function resetVehicleForm() {
	const f = document.getElementById("vehicleForm");
	f.reset();
	f.classList.remove("was-validated");
	f.querySelectorAll(".is-invalid").forEach((x) =>
		x.classList.remove("is-invalid"),
	);
	document.getElementById("vehicleFormError").classList.add("d-none");
}

function openVehicleAdd() {
	vehicleEditingId = null;
	resetVehicleForm();
	document.getElementById("vehicleModalTitle").textContent = "Add Vehicle";
	vehicleModal.show();
}

async function editVehicle(id) {
	try {
		const v = await apiGet(`/vehicles/${id}`);
		vehicleEditingId = id;
		document.getElementById("vehicleModalTitle").textContent = "Edit Vehicle";
		for (const [id, val] of [
			["registrationNumber", v.registrationNumber],
			["vin", v.vin],
			["make", v.make],
			["model", v.model],
			["year", v.year],
			["fuelType", v.fuelType],
			["mileage", v.mileage],
			["color", v.color || ""],
			["customerId", v.customerId],
		])
			document.getElementById(id).value = val;
		vehicleModal.show();
	} catch (e) {
		alertBox("vehicleAlert", e.message, "danger");
	}
}

function validateVehicleForm() {
	const f = document.getElementById("vehicleForm");
	const registration = document.getElementById("registrationNumber");
	const vinInput = document.getElementById("vin");
	const makeInput = document.getElementById("make");
	const modelInput = document.getElementById("model");
	const yearInput = document.getElementById("year");
	const mileageInput = document.getElementById("mileage");
	const colorInput = document.getElementById("color");
	const customerInput = document.getElementById("customerId");
	const nowYear = new Date().getFullYear();
	yearInput.max = String(nowYear + 1);
	const vin = vinInput.value.trim().toUpperCase();
	vinInput.value = vin;
	const errors = [];
	if (!registration.value.trim())
		errors.push("Registration Number is required.");
	else if (
		!/^[A-Za-z]{2}\s\d{2}\s[A-Za-z]{2}\s\d{4}$/.test(
			registration.value.trim(),
		)
	)
		errors.push("Registration Number format must be AB 12 CD 3456");
	if (!/^[A-HJ-NPR-Z0-9]{17}$/.test(vin))
		errors.push(
			"VIN must contain exactly 17 valid letters/numbers; I, O and Q are not allowed.",
		);
	if (makeInput.value.trim().length < 2 || makeInput.value.trim().length > 30)
		errors.push("Make must be 2-30 characters.");
	if (
		modelInput.value.trim().length < 2 ||
		modelInput.value.trim().length > 30
	)
		errors.push("Model must be 2–30 characters.");
	const year = Number(yearInput.value);
	if (!Number.isInteger(year) || year < 1886 || year > nowYear + 1)
		errors.push(`Year must be between 1886 and ${nowYear + 1}.`);
	if (
		!["Petrol", "Diesel", "Electric", "Hybrid", "CNG"].includes(
			document.getElementById("fuelType").value,
		)
	)
		errors.push("Select a valid fuel type.");
	const mileage = Number(mileageInput.value);
	if (!Number.isFinite(mileage) || mileage < 0 || mileage > 1000000)
		errors.push("Mileage must be between 0 and 1,000,000 km.");
	if (colorInput.value.trim().length > 30)
		errors.push("Color cannot exceed 30 characters.");
	if (!customerInput.value || Number(customerInput.value) <= 0)
		errors.push("Select a customer.");
	f.classList.add("was-validated");
	return errors;
}

async function saveVehicle(e) {
	e.preventDefault();
	const f = document.getElementById("vehicleForm"),
		err = document.getElementById("vehicleFormError");
	err.classList.add("d-none");
	const errors = validateVehicleForm();
	if (errors.length) {
		err.innerHTML = errors.map((x) => `<div>${esc(x)}</div>`).join("");
		err.classList.remove("d-none");
		return;
	}
	const data = {
		registrationNumber: document
			.getElementById("registrationNumber")
			.value.trim()
			.toUpperCase(),
		vin: document.getElementById("vin").value.trim().toUpperCase(),
		make: document.getElementById("make").value.trim(),
		model: document.getElementById("model").value.trim(),
		year: Number(document.getElementById("year").value),
		fuelType: document.getElementById("fuelType").value,
		mileage: Number(document.getElementById("mileage").value),
		color: document.getElementById("color").value.trim() || null,
		customerId: Number(document.getElementById("customerId").value),
	};
	const btn = document.getElementById("saveVehicleBtn");
	setBusy(btn, true);
	try {
		if (vehicleEditingId) await apiPut(`/vehicles/${vehicleEditingId}`, data);
		else await apiPost("/vehicles", data);
		vehicleModal.hide();
		await loadVehicles();
		alertBox(
			"vehicleAlert",
			vehicleEditingId
				? "Vehicle updated successfully."
				: "Vehicle created successfully.",
		);
	} catch (e) {
		err.textContent = e.message;
		err.classList.remove("d-none");
	} finally {
		setBusy(btn, false);
	}
}

async function deleteVehicle(id) {
	if (!confirm("Delete this vehicle?")) return;
	try {
		await apiDelete(`/vehicles/${id}`);
		await loadVehicles();
		alertBox("vehicleAlert", "Vehicle deleted successfully.");
	} catch (e) {
		alertBox("vehicleAlert", e.message, "danger");
	}
}

async function viewVehicle(id) {
	try {
		const v = await apiGet(`/vehicles/${id}`),
			h = await apiGet(`/vehicles/${id}/service-history`),
			d = await apiGet(`/vehicles/${id}/maintenance-due`);
		document.getElementById("historyTitle").textContent =
			`${v.registrationNumber} — ${v.make} ${v.model}`;
		document.getElementById("historyBody").innerHTML =
			`<div class="row g-3 mb-4"><div class="col-md-3"><strong>Customer</strong><div>${esc(v.customerName)}</div></div><div class="col-md-3"><strong>Mileage</strong><div>${Number(v.mileage).toLocaleString("en-IN")} km</div></div><div class="col-md-3"><strong>Last Service</strong><div>${dateOnly(d.lastServiceDate)}</div></div><div class="col-md-3"><strong>Maintenance</strong><div class="${d.isDue ? "text-danger" : "text-success"} fw-semibold">${esc(d.message)}</div></div></div><h6>Service History</h6><div class="table-responsive"><table class="table table-sm"><thead><tr><th>Date</th><th>Job</th><th>Service</th><th>Mileage</th><th>Cost</th><th>Status</th></tr></thead><tbody>${(h.history || []).map((x) => `<tr><td>${dateOnly(x.serviceDate)}</td><td>${esc(x.jobNumber || "-")}</td><td>${esc(x.serviceName)}</td><td>${Number(x.mileage).toLocaleString("en-IN")}</td><td>${money(x.cost)}</td><td>${esc(x.status || "-")}</td></tr>`).join("") || '<tr><td colspan="6" class="text-center text-muted">No service history.</td></tr>'}</tbody></table></div>`;
		historyModal.show();
	} catch (e) {
		alertBox("vehicleAlert", e.message, "danger");
	}
}
