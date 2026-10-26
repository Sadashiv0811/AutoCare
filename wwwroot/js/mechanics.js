let mechanics = [],
	mEdit = null,
	mPage = 1,
	mModal;

const mPerPage = 5;

document.addEventListener("DOMContentLoaded", async () => {
	mModal = new bootstrap.Modal("#mechanicModal");

	document.getElementById("mechanicSearch").addEventListener("input", () => {
		mPage = 1;
		renderMechanics();
	});

	document.getElementById("mechanicStatus").addEventListener("change", () => {
		mPage = 1;
		renderMechanics();
	});

	document
		.getElementById("toggleMechanicPassword")
		.addEventListener("click", toggleMechanicPassword);

	document
		.getElementById("addMechanicBtn")
		.addEventListener("click", openMechanicAdd);

	document
		.getElementById("mechanicForm")
		.addEventListener("submit", saveMechanic);

	if (role() !== "Admin") {
		document.getElementById("addMechanicBtn").classList.add("d-none");
	}

	await loadMechanics();
});

async function loadMechanics() {
	try {
		mechanics = (await apiGet("/mechanics")) || [];

		renderMechanics();

		mTotal.textContent = mechanics.length;

		mActive.textContent = mechanics.filter((x) => x.isActive).length;

		mInactive.textContent = mechanics.filter((x) => !x.isActive).length;

		mJobs.textContent = mechanics.reduce(
			(a, x) => a + (x.serviceJobCount || 0),
			0,
		);
	} catch (e) {
		alertBox("mechanicAlert", e.message, "danger");
	}
}

function renderMechanics() {
	const q = mechanicSearch.value.toLowerCase().trim();
	const st = mechanicStatus.value;

	const list = mechanics.filter(
		(x) =>
			(!q ||
				[x.fullName, x.phone, x.email, x.specialization].some((v) =>
					String(v || "")
						.toLowerCase()
						.includes(q),
				)) &&
			(!st || String(x.isActive) === st),
	);

	const pages = Math.max(1, Math.ceil(list.length / mPerPage));

	if (mPage > pages) {
		mPage = pages;
	}

	const page = list.slice((mPage - 1) * mPerPage, mPage * mPerPage);

	mechanicBody.innerHTML = page.length
		? page
				.map(
					(x) => `
						<tr>
							<td>${x.id}</td>

							<td>
								<strong>${esc(x.fullName)}</strong>
								<br>
								<small>
									${esc(x.email || "-")} · ${esc(x.phone)}
								</small>
							</td>

							<td>
								${esc(x.specialization || "-")}
							</td>

							<td>
								${x.experienceYears} yrs
							</td>

							<td>
								${x.serviceJobCount || 0}
							</td>

							<td>
								<span class="badge ${x.isActive ? "bg-success" : "bg-secondary"}">
									${x.isActive ? "Active" : "Inactive"}
								</span>
							</td>

							<td>
								${
									role() === "Admin"
										? `
											<button
												class="btn btn-sm btn-outline-primary me-1"
												onclick="editMechanic(${x.id})"
											>
												<i class="bi bi-pencil"></i>
											</button>

											<button
												class="btn btn-sm btn-outline-danger"
												onclick="deleteMechanic(${x.id})"
											>
												<i class="bi bi-trash"></i>
											</button>
										`
										: "-"
								}
							</td>
						</tr>
					`,
				)
				.join("")
		: `
			<tr>
				<td
					colspan="7"
					class="text-center text-muted py-4"
				>
					No mechanics found.
				</td>
			</tr>
		`;

	mechanicInfo.textContent = `Showing ${
		list.length ? (mPage - 1) * mPerPage + 1 : 0
	} to ${Math.min(mPage * mPerPage, list.length)} of ${list.length} mechanics`;

	renderPagination(pages);
}

function renderPagination(pages) {
	const p = document.getElementById("mechanicPages");

	p.innerHTML = "";

	if (pages <= 1) return;

	// Previous
	addNavigationButton("Previous", mPage > 1, () => {
		mPage--;
		renderMechanics();
	});

	// Always show page 1
	addPageButton(1);

	// Pages around current page
	const startPage = Math.max(2, mPage - 1);
	const endPage = Math.min(pages - 1, mPage + 1);

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
	addNavigationButton("Next", mPage < pages, () => {
		mPage++;
		renderMechanics();
	});

	function addPageButton(pageNumber) {
		const li = document.createElement("li");

		li.className = `page-item ${pageNumber === mPage ? "active" : ""}`;

		li.innerHTML = `
			<button class="page-link">
				${pageNumber}
			</button>
		`;

		li.querySelector("button").onclick = () => {
			mPage = pageNumber;
			renderMechanics();
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
			<button class="page-link">
				${text}
			</button>
		`;

		if (enabled) {
			li.querySelector("button").onclick = action;
		}

		p.appendChild(li);
	}
}

function openMechanicAdd() {
	mEdit = null;

	mechanicForm.reset();

	mechanicTitle.textContent = "Add Mechanic";
	saveMechanicBtn.textContent = "Create Mechanic";

	// Password is required when creating
	newMechanicPasswordField.classList.remove("d-none");
	mechanicPassword.disabled = false;

	// Clear validation state
	mechanicForm.classList.remove("was-validated");
	mechanicFormError.classList.add("d-none");
	mechanicFormError.textContent = "";

	resetMechanicPasswordVisibility();

	mModal.show();
}

async function editMechanic(id) {
	try {
		const x = await apiGet(`/mechanics/${id}`);

		mEdit = id;

		mechanicTitle.textContent = "Edit Mechanic";
		saveMechanicBtn.textContent = "Save Changes";

		mechanicName.value = x.fullName;
		mechanicPhone.value = x.phone;
		mechanicEmail.value = x.email || "";
		specialization.value = x.specialization || "";
		experienceYears.value = x.experienceYears;
		mechanicActive.value = String(x.isActive);

		// Password is not edited
		newMechanicPasswordField.classList.add("d-none");
		mechanicPassword.disabled = true;
		mechanicPassword.value = "";

		// Clear validation state
		mechanicForm.classList.remove("was-validated");
		mechanicFormError.classList.add("d-none");
		mechanicFormError.textContent = "";

		mModal.show();
	} catch (e) {
		alertBox("mechanicAlert", e.message, "danger");
	}
}

async function saveMechanic(e) {
	e.preventDefault();

	const errors = validateMechanicForm();

	if (errors.length > 0) {
		mechanicFormError.innerHTML = errors
			.map((error) => `<div>${esc(error)}</div>`)
			.join("");

		mechanicFormError.classList.remove("d-none");

		return;
	}

	mechanicFormError.classList.add("d-none");
	mechanicFormError.textContent = "";

	const btn = saveMechanicBtn;

	setBusy(btn, true);

	try {
		let userId;

		const email = mechanicEmail.value.trim().toLowerCase();

		if (!mEdit) {
			// Create User account
			const u = await apiPost("/users", {
				fullName: mechanicName.value.trim(),
				email: email,
				password: mechanicPassword.value,
				role: "Mechanic",
				isActive: true,
			});

			userId = u.id;

			// Create Mechanic profile
			await apiPost("/mechanics", {
				userId,
				fullName: mechanicName.value.trim(),
				phone: mechanicPhone.value.trim(),
				email: email,
				specialization: specialization.value.trim() || null,
				experienceYears: Number(experienceYears.value),
				isActive: mechanicActive.value === "true",
			});
		} else {
			await apiPut(`/mechanics/${mEdit}`, {
				fullName: mechanicName.value.trim(),
				phone: mechanicPhone.value.trim(),
				email: email,
				specialization: specialization.value.trim() || null,
				experienceYears: Number(experienceYears.value),
				isActive: mechanicActive.value === "true",
			});
		}

		mModal.hide();

		await loadMechanics();

		alertBox(
			"mechanicAlert",
			mEdit
				? "Mechanic updated successfully."
				: "Mechanic created successfully.",
		);
	} catch (e) {
		mechanicFormError.textContent = e.message;
		mechanicFormError.classList.remove("d-none");
	} finally {
		setBusy(btn, false);
	}
}

async function deleteMechanic(id) {
	if (!confirm("Delete this mechanic profile?")) return;

	try {
		await apiDelete(`/mechanics/${id}`);

		await loadMechanics();

		alertBox("mechanicAlert", "Mechanic deleted successfully.");
	} catch (e) {
		alertBox("mechanicAlert", e.message, "danger");
	}
}

function validateMechanicForm() {
	const f = document.getElementById("mechanicForm");

	const nameInput = document.getElementById("mechanicName");

	const phoneInput = document.getElementById("mechanicPhone");

	const emailInput = document.getElementById("mechanicEmail");

	const passwordInput = document.getElementById("mechanicPassword");

	const specializationInput = document.getElementById("specialization");

	const experienceInput = document.getElementById("experienceYears");

	const activeInput = document.getElementById("mechanicActive");

	const errors = [];

	const name = nameInput.value.trim();
	const phone = phoneInput.value.trim();
	const email = emailInput.value.trim();
	const specialization = specializationInput.value.trim();

	const experience = Number(experienceInput.value);

	// Full Name
	if (!name) {
		errors.push("Full Name is required.");
	} else if (name.length < 4 || name.length > 50) {
		errors.push("Full Name must be 4-50 characters.");
	}

	// Phone
	if (!phone) {
		errors.push("Phone number is required.");
	} else if (!/^[6-9]\d{9}$/.test(phone)) {
		errors.push("Please enter a valid 10-digit mobile number.");
	}

	// Email
	if (!email) {
		errors.push("Email is required.");
	} else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
		errors.push("Please enter a valid email address.");
	}

	// Password - only required while adding
	if (!mEdit) {
		const password = passwordInput.value;

		if (!password) {
			errors.push("Password is required.");
		} else if (password.length < 6) {
			errors.push("Password must be at least 6 characters.");
		}
	}

	// Specialization
	if (!specialization) {
		errors.push("Specialization is required.");
	} else if (specialization.length > 50) {
		errors.push("Specialization cannot exceed 50 characters.");
	}

	// Experience
	 if (!Number.isInteger(experience) || experience < 0 || experience > 50) {
		errors.push("Experience must be between 0 and 50 years.");
	}

	// Status
	if (!["true", "false"].includes(activeInput.value)) {
		errors.push("Select a valid status.");
	}

	f.classList.add("was-validated");

	return errors;
}

function toggleMechanicPassword() {
	const passwordInput = document.getElementById("mechanicPassword");

	const toggleButton = document.getElementById("toggleMechanicPassword");

	const icon = toggleButton.querySelector("i");

	if (passwordInput.type === "password") {
		passwordInput.type = "text";

		icon.classList.remove("bi-eye");

		icon.classList.add("bi-eye-slash");

		toggleButton.setAttribute("aria-label", "Hide password");
	} else {
		passwordInput.type = "password";

		icon.classList.remove("bi-eye-slash");

		icon.classList.add("bi-eye");

		toggleButton.setAttribute("aria-label", "Show password");
	}
}

function resetMechanicPasswordVisibility() {
	const passwordInput = document.getElementById("mechanicPassword");

	const toggleButton = document.getElementById("toggleMechanicPassword");

	const icon = toggleButton.querySelector("i");

	passwordInput.type = "password";

	icon.classList.remove("bi-eye-slash");

	icon.classList.add("bi-eye");

	toggleButton.setAttribute("aria-label", "Show password");
}
