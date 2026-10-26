let customers = [];

let currentPage = 1;
const customersPerPage = 5;

let editingCustomerId = null;
let deletingCustomerId = null;

let customerModal;
let viewCustomerModal;
let deleteCustomerModal;

// PAGE LOAD
document.addEventListener("DOMContentLoaded", () => {
	customerModal = new bootstrap.Modal(
		document.getElementById("customerModal"),
	);

	viewCustomerModal = new bootstrap.Modal(
		document.getElementById("viewCustomerModal"),
	);

	deleteCustomerModal = new bootstrap.Modal(
		document.getElementById("deleteCustomerModal"),
	);

	document
		.getElementById("addCustomerButton")
		.addEventListener("click", openAddCustomerModal);

	document
		.getElementById("customerForm")
		.addEventListener("submit", handleCustomerSubmit);

	document
		.getElementById("customerSearch")
		.addEventListener("input", handleSearch);

	document
		.getElementById("clearCustomerSearch")
		.addEventListener("click", clearSearch);

	document
		.getElementById("confirmDeleteButton")
		.addEventListener("click", confirmDelete);

	document
		.getElementById("toggleCustomerPassword")
		.addEventListener("click", toggleCustomerPassword);

	// Remove validation while user corrects the fields
	document
		.getElementById("customerFullName")
		.addEventListener("input", () =>
			clearFieldValidation("customerFullName"),
		);

	document
		.getElementById("customerEmail")
		.addEventListener("input", () => clearFieldValidation("customerEmail"));

	document
		.getElementById("customerPhone")
		.addEventListener("input", () => clearFieldValidation("customerPhone"));

	document
		.getElementById("customerPassword")
		.addEventListener("input", () =>
			clearFieldValidation("customerPassword"),
		);

	loadCustomers();
});

// LOAD CUSTOMERS
async function loadCustomers() {
	try {
		const data = await apiGet("/customers");

		customers = Array.isArray(data) ? data : [];

		updateSummaryCards();

		displayCustomers();
	} catch (error) {
		showPageAlert(error.message || "Unable to load customers.", "danger");
	}
}

// SUMMARY CARDS
function updateSummaryCards() {
	document.getElementById("customerCount").textContent = customers.length;

	const totalVehicles = customers.reduce(
		(total, customer) => total + (customer.vehicleCount || 0),
		0,
	);

	document.getElementById("customerVehicleCount").textContent = totalVehicles;

	const now = new Date();

	const newCustomers = customers.filter((customer) => {
		if (!customer.createdAt) {
			return false;
		}

		const createdDate = new Date(customer.createdAt);

		return (
			createdDate.getMonth() === now.getMonth() &&
			createdDate.getFullYear() === now.getFullYear()
		);
	});

	document.getElementById("newCustomersThisMonth").textContent =
		newCustomers.length;
}

// SEARCH
function handleSearch() {
	currentPage = 1;

	displayCustomers();
}

function clearSearch() {
	const searchInput = document.getElementById("customerSearch");

	searchInput.value = "";

	currentPage = 1;

	displayCustomers();
}

// FILTER CUSTOMERS
function getFilteredCustomers() {
	const searchValue = document
		.getElementById("customerSearch")
		.value.trim()
		.toLowerCase();

	if (!searchValue) {
		return customers;
	}

	return customers.filter((customer) => {
		const name = customer.fullName?.toLowerCase() || "";

		const phone = customer.phone?.toLowerCase() || "";

		const email = customer.email?.toLowerCase() || "";

		return (
			name.includes(searchValue) ||
			phone.includes(searchValue) ||
			email.includes(searchValue)
		);
	});
}

// DISPLAY CUSTOMERS
function displayCustomers() {
	const tableBody = document.getElementById("customersTableBody");

	const filteredCustomers = getFilteredCustomers();

	const totalPages = Math.ceil(filteredCustomers.length / customersPerPage);

	if (totalPages > 0 && currentPage > totalPages) {
		currentPage = totalPages;
	}

	const startIndex = (currentPage - 1) * customersPerPage;

	const endIndex = startIndex + customersPerPage;

	const pageCustomers = filteredCustomers.slice(startIndex, endIndex);

	tableBody.innerHTML = "";

	if (pageCustomers.length === 0) {
		tableBody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-muted py-4">
                    No customers found.
                </td>
            </tr>
        `;
	} else {
		pageCustomers.forEach((customer) => {
			const row = document.createElement("tr");

			row.innerHTML = `
                <td>${escapeHtml(customer.id)}</td>

                <td>
                    <strong>
                        ${escapeHtml(customer.fullName)}
                    </strong>
                </td>

                <td>
                    ${escapeHtml(customer.phone || "-")}
                </td>

                <td>
                    ${escapeHtml(customer.email || "-")}
                </td>

                <td>
                    <span class="badge bg-info">
                        ${customer.vehicleCount || 0}
                    </span>
                </td>

                <td>

                    <button
                        class="btn btn-sm btn-outline-info me-1"
                        onclick="viewCustomer(${customer.id})"
                        title="View"
                    >
                        <i class="bi bi-eye"></i>
                    </button>


                    <button
                        class="btn btn-sm btn-outline-primary me-1"
                        onclick="editCustomer(${customer.id})"
                        title="Edit"
                    >
                        <i class="bi bi-pencil"></i>
                    </button>


                    <button
                        class="btn btn-sm btn-outline-danger"
                        onclick="deleteCustomer(${customer.id})"
                        title="Delete"
                    >
                        <i class="bi bi-trash"></i>
                    </button>

                </td>
            `;

			tableBody.appendChild(row);
		});
	}

	updatePaginationInfo(
		filteredCustomers.length,
		startIndex,
		pageCustomers.length,
	);

	renderPagination(totalPages);
}

// PAGINATION INFO
function updatePaginationInfo(totalCustomers, startIndex, visibleCount) {
	const info = document.getElementById("customerPaginationInfo");

	if (totalCustomers === 0) {
		info.textContent = "Showing 0 customers";

		return;
	}

	const firstCustomer = startIndex + 1;

	const lastCustomer = startIndex + visibleCount;

	info.textContent = `Showing ${firstCustomer} to ${lastCustomer} of ${totalCustomers} customers`;
}

// PAGINATION
function renderPagination(totalPages) {
	const pagination = document.getElementById("customerPagination");

	pagination.innerHTML = "";

	if (totalPages <= 1) {
		return;
	}

	// Previous
	addNavigationButton("Previous", currentPage > 1, () => {
		currentPage--;
		displayCustomers();
	});

	// Always show page 1
	addPageButton(1);

	// Pages around current page
	const startPage = Math.max(2, currentPage - 1);
	const endPage = Math.min(totalPages - 1, currentPage + 1);

	// Left dots
	if (startPage > 2) {
		addDots();
	}

	// Current page and nearby pages
	for (let page = startPage; page <= endPage; page++) {
		addPageButton(page);
	}

	// Right dots
	if (endPage < totalPages - 1) {
		addDots();
	}

	// Always show last page
	if (totalPages > 1) {
		addPageButton(totalPages);
	}

	// Next
	addNavigationButton("Next", currentPage < totalPages, () => {
		currentPage++;
		displayCustomers();
	});

	// Page button
	function addPageButton(pageNumber) {
		const li = document.createElement("li");

		li.className = `page-item ${pageNumber === currentPage ? "active" : ""}`;

		li.innerHTML = `
			<button class="page-link">${pageNumber}</button>
		`;

		li.querySelector("button").addEventListener("click", () => {
			currentPage = pageNumber;
			displayCustomers();
		});

		pagination.appendChild(li);
	}

	// Dots
	function addDots() {
		const li = document.createElement("li");

		li.className = "page-item disabled";

		li.innerHTML = `
			<span class="page-link">...</span>
		`;

		pagination.appendChild(li);
	}

	// Previous / Next
	function addNavigationButton(text, enabled, action) {
		const li = document.createElement("li");

		li.className = `page-item ${enabled ? "" : "disabled"}`;

		li.innerHTML = `
			<button class="page-link">${text}</button>
		`;

		if (enabled) {
			li.querySelector("button").addEventListener("click", action);
		}

		pagination.appendChild(li);
	}
}

// ADD CUSTOMER
function openAddCustomerModal() {
	editingCustomerId = null;

	document.getElementById("customerModalLabel").textContent = "Add Customer";

	document.getElementById("saveCustomerButton").textContent =
		"Create Customer";

	document.getElementById("customerForm").reset();

	clearAllValidation();

	document.getElementById("customerPasswordGroup").classList.remove("d-none");

	document.getElementById("customerPassword").required = true;

	resetCustomerPasswordVisibility();

	document.getElementById("customerFormError").classList.add("d-none");

	customerModal.show();
}

// EDIT CUSTOMER
async function editCustomer(id) {
	try {
		const customer = await apiGet(`/customers/${id}`);

		if (!customer) {
			return;
		}

		editingCustomerId = id;

		document.getElementById("customerModalLabel").textContent =
			"Edit Customer";

		document.getElementById("saveCustomerButton").textContent =
			"Save Changes";

		document.getElementById("customerFullName").value =
			customer.fullName || "";

		document.getElementById("customerEmail").value = customer.email || "";

		document.getElementById("customerPhone").value = customer.phone || "";

		document.getElementById("customerAddress").value = customer.address || "";

		document.getElementById("customerPassword").value = "";

		document.getElementById("customerPasswordGroup").classList.add("d-none");

		document.getElementById("customerPassword").required = false;

		clearAllValidation();

		document.getElementById("customerFormError").classList.add("d-none");

		customerModal.show();
	} catch (error) {
		showPageAlert(error.message || "Unable to load customer.", "danger");
	}
}

// SUBMIT CUSTOMER
async function handleCustomerSubmit(event) {
	event.preventDefault();

	const isValid = validateCustomerForm();

	if (!isValid) {
		return;
	}

	const fullName = document.getElementById("customerFullName").value.trim();

	const email = document.getElementById("customerEmail").value.trim();

	const phone = document.getElementById("customerPhone").value.trim();

	const password = document.getElementById("customerPassword").value;

	const address = document.getElementById("customerAddress").value.trim();

	const saveButton = document.getElementById("saveCustomerButton");

	const formError = document.getElementById("customerFormError");

	saveButton.disabled = true;

	saveButton.innerHTML = `
        <span
            class="spinner-border spinner-border-sm me-1"
            role="status"
        ></span>
        Saving...
    `;

	formError.classList.add("d-none");

	try {
		// ==========================================
		// CREATE
		// ==========================================

		if (editingCustomerId === null) {
			const registerResponse = await apiPost("/auth/register", {
				fullName: fullName,
				email: email,
				password: password,
			});

			const customerId = registerResponse.customerId;

			if (!customerId) {
				throw new Error("Customer ID was not returned by registration.");
			}

			await apiPut(`/customers/${customerId}`, {
				fullName: fullName,
				phone: phone,
				email: email || null,
				address: address || null,
			});

			showPageAlert("Customer created successfully.", "success");
		}

		// ==========================================
		// UPDATE
		// ==========================================
		else {
			await apiPut(`/customers/${editingCustomerId}`, {
				fullName: fullName,
				phone: phone,
				email: email || null,
				address: address || null,
			});

			showPageAlert("Customer updated successfully.", "success");
		}

		customerModal.hide();

		await loadCustomers();
	} catch (error) {
		formError.textContent = error.message || "Unable to save customer.";

		formError.classList.remove("d-none");
	} finally {
		saveButton.disabled = false;

		saveButton.textContent =
			editingCustomerId === null ? "Create Customer" : "Save Changes";
	}
}

// CUSTOMER FORM VALIDATION
function validateCustomerForm() {
	let isValid = true;

	const fullName = document.getElementById("customerFullName");

	const email = document.getElementById("customerEmail");

	const phone = document.getElementById("customerPhone");

	const password = document.getElementById("customerPassword");

	// ==========================================
	// FULL NAME
	// ==========================================

	const fullNameValue = fullName.value.trim();

	if (fullNameValue.length < 2 || !/^[A-Za-zÀ-ÿ\s]+$/.test(fullNameValue)) {
		setInvalid(fullName, "Please enter a valid full name.");

		isValid = false;
	} else {
		setValid(fullName);
	}

	// ==========================================
	// EMAIL
	// ==========================================

	const emailValue = email.value.trim();

	if (emailValue !== "") {
		const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

		if (!emailPattern.test(emailValue)) {
			setInvalid(email, "Please enter a valid email address.");

			isValid = false;
		} else {
			setValid(email);
		}
	} else {
		clearFieldValidation("customerEmail");
	}

	// ==========================================
	// PHONE
	// ==========================================

	const phoneValue = phone.value.trim();

	if (!/^[6-9]\d{9}$/.test(phoneValue)) {
		setInvalid(phone, "Please enter a valid 10-digit mobile number.");

		isValid = false;
	} else {
		setValid(phone);
	}

	// ==========================================
	// PASSWORD
	// ONLY REQUIRED WHEN ADDING
	// ==========================================

	if (editingCustomerId === null) {
		if (password.value.length < 6) {
			setInvalid(password, "Password must be at least 6 characters.");

			isValid = false;
		} else {
			setValid(password);
		}
	} else {
		clearFieldValidation("customerPassword");
	}

	return isValid;
}

// VALIDATION HELPERS
function setInvalid(element, message) {
	element.classList.remove("is-valid");
	element.classList.add("is-invalid");

	const container =
		element.closest(".input-group")?.parentElement || element.parentElement;

	const feedback = container.querySelector(".invalid-feedback");

	if (feedback) {
		feedback.textContent = message;
	}
}

function setValid(element) {
	element.classList.remove("is-invalid");

	element.classList.add("is-valid");
}

function clearFieldValidation(id) {
	const element = document.getElementById(id);

	element.classList.remove("is-invalid", "is-valid");
}

function clearAllValidation() {
	[
		"customerFullName",
		"customerEmail",
		"customerPhone",
		"customerPassword",
	].forEach((id) => {
		clearFieldValidation(id);
	});
}

// CUSTOMER PASSWORD TOGGLE
function toggleCustomerPassword() {
	const passwordInput = document.getElementById("customerPassword");

	const toggleButton = document.getElementById("toggleCustomerPassword");

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

function resetCustomerPasswordVisibility() {
	const passwordInput = document.getElementById("customerPassword");

	const toggleButton = document.getElementById("toggleCustomerPassword");

	const icon = toggleButton.querySelector("i");

	passwordInput.type = "password";

	icon.classList.remove("bi-eye-slash");

	icon.classList.add("bi-eye");

	toggleButton.setAttribute("aria-label", "Show password");
}

// VIEW CUSTOMER
async function viewCustomer(id) {
	try {
		const customer = await apiGet(`/customers/${id}`);

		if (!customer) {
			return;
		}

		const details = document.getElementById("customerDetails");

		const createdDate = customer.createdAt
			? formatDate(customer.createdAt)
			: "-";

		details.innerHTML = `

            <div class="row g-3">

                <div class="col-6">
                    <strong>ID</strong>
                    <div>${escapeHtml(customer.id)}</div>
                </div>

                <div class="col-6">
                    <strong>Vehicles</strong>
                    <div>${customer.vehicleCount || 0}</div>
                </div>

                <div class="col-12">
                    <strong>Full Name</strong>
                    <div>${escapeHtml(customer.fullName)}</div>
                </div>

                <div class="col-12">
                    <strong>Email</strong>
                    <div>${escapeHtml(customer.email || "-")}</div>
                </div>

                <div class="col-12">
                    <strong>Phone</strong>
                    <div>${escapeHtml(customer.phone || "-")}</div>
                </div>

                <div class="col-12">
                    <strong>Address</strong>
                    <div>${escapeHtml(customer.address || "-")}</div>
                </div>

                <div class="col-12">
                    <strong>Created</strong>
                    <div>${createdDate}</div>
                </div>

            </div>

        `;

		viewCustomerModal.show();
	} catch (error) {
		showPageAlert(error.message || "Unable to load customer.", "danger");
	}
}

// DELETE CUSTOMER
function deleteCustomer(id) {
	deletingCustomerId = id;

	deleteCustomerModal.show();
}

async function confirmDelete() {
	if (!deletingCustomerId) {
		return;
	}

	const deleteButton = document.getElementById("confirmDeleteButton");

	deleteButton.disabled = true;

	deleteButton.innerHTML = `
        <span
            class="spinner-border spinner-border-sm me-1"
            role="status"
        ></span>
        Deleting...
    `;

	try {
		await apiDelete(`/customers/${deletingCustomerId}`);

		deleteCustomerModal.hide();

		showPageAlert("Customer deleted successfully.", "success");

		deletingCustomerId = null;

		await loadCustomers();
	} catch (error) {
		deleteCustomerModal.hide();

		showPageAlert(error.message || "Unable to delete customer.", "danger");
	} finally {
		deleteButton.disabled = false;

		deleteButton.textContent = "Delete Customer";
	}
}

// PAGE ALERT
function showPageAlert(message, type) {
	const alert = document.getElementById("customerPageAlert");

	alert.className = `alert alert-${type}`;

	alert.textContent = message;

	alert.classList.remove("d-none");

	setTimeout(() => {
		alert.classList.add("d-none");
	}, 4000);
}

// FORMAT DATE
function formatDate(dateValue) {
	const date = new Date(dateValue);

	if (isNaN(date.getTime())) {
		return "-";
	}

	return date.toLocaleDateString("en-IN", {
		day: "2-digit",
		month: "short",
		year: "numeric",
	});
}

// HTML ESCAPE
function escapeHtml(value) {
	if (value === null || value === undefined) {
		return "";
	}

	return String(value)
		.replaceAll("&", "&amp;")
		.replaceAll("<", "&lt;")
		.replaceAll(">", "&gt;")
		.replaceAll('"', "&quot;")
		.replaceAll("'", "&#039;");
}
