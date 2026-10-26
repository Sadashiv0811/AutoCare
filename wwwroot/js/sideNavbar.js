// Run authentication check
requireAuth();

document.addEventListener("DOMContentLoaded", async function () {
	const container = document.getElementById("sidebarContainer");

	if (!container) {
		return;
	}

	try {
		const response = await fetch("sideNavbar.html");

		if (!response.ok) {
			throw new Error("Unable to load sidebar.");
		}

		container.innerHTML = await response.text();

		setupSidebar();
		setupSidebarToggle();
	} catch (error) {
		console.error("Sidebar Error:", error);
	}

	const userName = document.getElementById("userName");

	if (userName) {
		const name = getUserName();
		const role = getUserRole();

		if (name && role) {
			userName.innerHTML = "<strong>" + role + "</strong> - " + name;
		}
	}

	// Logout
	const logoutButton = document.getElementById("logoutButton");

	if (logoutButton) {
		logoutButton.addEventListener("click", function () {
			logout();
		});
	}
});

function setupSidebar() {
	const role = localStorage.getItem("userRole");

	if (!role) {
		return;
	}

	// Role-based visibility
	const navItems = document.querySelectorAll("#sidebar [data-roles]");

	navItems.forEach(function (item) {
		const allowedRoles = item.dataset.roles
			.split(",")
			.map((role) => role.trim());

		if (!allowedRoles.includes(role)) {
			item.remove();
		}
	});

	// Active page
	const currentPage = window.location.pathname.split("/").pop().toLowerCase();

	const navLinks = document.querySelectorAll("#sidebar .nav-link");

	navLinks.forEach(function (link) {
		const linkPage = link.getAttribute("href").split("/").pop().toLowerCase();

		if (linkPage === currentPage) {
			link.classList.add("active");
			link.classList.remove("text-white");
		}
	});
}

function setupSidebarToggle() {
	const sidebar = document.getElementById("sidebar");
	const container = document.getElementById("sidebarContainer");
	const toggleButton = document.getElementById("sidebarToggle");

	if (!sidebar || !container || !toggleButton) {
		return;
	}

	toggleButton.addEventListener("click", function () {
		sidebar.classList.toggle("collapsed");
		container.classList.toggle("sidebar-collapsed");
	});
}

// Check authentication
function requireAuth() {
	const token = getToken();

	if (!token) {
		window.location.replace("/pages/login.html");

		return false;
	}

	return true;
}

// Get Values
function getToken() {
	// Get stored JWT
	return localStorage.getItem("token");
}

function getUserEmail() {
	return localStorage.getItem("userEmail");
}

function getUserName() {
	return localStorage.getItem("userName");
}

function getUserRole() {
	return localStorage.getItem("userRole");
}

// Logout
function logout() {
	localStorage.clear();
	window.location.replace("/pages/login.html");
}
