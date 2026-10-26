// Login
//  ↓
// Store token + email
//  ↓
// Go to dashboard

// IF ALREADY LOGGED IN
const token = localStorage.getItem("token");

if (token) {
  const role = localStorage.getItem("userRole");

  if (role === "Admin" || role === "Service Advisor") {
    window.location.replace("/pages/dashboard.html");
  } else if (role === "Mechanic") {
    window.location.replace("/pages/services.html");
  } else if (role === "Customer") {
    window.location.replace("/pages/vehicles.html");
  }
}

// LOGIN FORM
const loginForm = document.getElementById("loginForm");

loginForm.addEventListener("submit", async function (event) {
  event.preventDefault();

  const emailInput = document.getElementById("email");

  const passwordInput = document.getElementById("password");

  const email = emailInput.value.trim();

  const password = passwordInput.value.trim();

  const errorMessage = document.getElementById("loginError");

  const loginButton = document.querySelector(
    "#loginForm button[type='submit']",
  );

  // Clear previous messages
  errorMessage.classList.add("d-none");

  errorMessage.textContent = "";

  // Clear previous validation
  clearValidation(emailInput);
  clearValidation(passwordInput);

  // VALIDATION
  let isValid = true;

  // Email
  if (!email) {
    setInvalid(emailInput, "Email is required.");

    isValid = false;
  } else {
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailPattern.test(email)) {
      setInvalid(emailInput, "Please enter a valid email address.");

      isValid = false;
    }
  }

  // Password
  if (!password) {
    setInvalid(passwordInput, "Password is required.");

    isValid = false;
  } else if (password.length < 6) {
    setInvalid(passwordInput, "Password must be at least 6 characters.");

    isValid = false;
  }

  // Stop here if validation failed
  if (!isValid) {
    return;
  }

  // LOGIN BUTTON
  loginButton.disabled = true;

  loginButton.innerHTML = `
            <span
                class="spinner-border spinner-border-sm me-1"
                role="status"
                aria-hidden="true"
            ></span>
            Logging in...
        `;

  try {
    const response = await fetch("/api/auth/login", {
      method: "POST",

      headers: {
        "Content-Type": "application/json",
      },

      body: JSON.stringify({
        email: email,
        password: password,
      }),
    });

    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.message || "Login failed.");
    }

    // STORE JWT
    localStorage.setItem("token", data.token);

    // STORE LOGGED-IN USER EMAIL
    localStorage.setItem("userEmail", email);

    // OPTIONAL USER INFORMATION
    if (data.fullName) {
      localStorage.setItem("userName", data.fullName);
    }

    if (data.role) {
      localStorage.setItem("userRole", data.role);
    }

    // REDIRECT BASED ON ROLE
    if (data.role === "Admin" || data.role === "Service Advisor") {
      window.location.replace("/pages/dashboard.html");
    } else if (data.role === "Mechanic") {
      window.location.replace("/pages/services.html");
    } else if (data.role === "Customer") {
      window.location.replace("/pages/vehicles.html");
    }
  } catch (error) {
    console.error("Login Error:", error);

    errorMessage.textContent = error.message || "Login failed.";

    errorMessage.classList.remove("d-none");
  } finally {
    loginButton.disabled = false;

    loginButton.textContent = "Login";
  }
});

// CLEAR LOGIN FIELDS WHEN PAGE IS SHOWN AGAIN
window.addEventListener("pageshow", function () {
  const loginForm = document.getElementById("loginForm");

  if (loginForm) {
    loginForm.reset();
  }

  const emailInput = document.getElementById("email");

  const passwordInput = document.getElementById("password");

  if (emailInput) {
    clearValidation(emailInput);
  }

  if (passwordInput) {
    clearValidation(passwordInput);
  }
});

// PASSWORD TOGGLE
document.addEventListener("DOMContentLoaded", () => {
  const toggleButton = document.getElementById("toggleLoginPassword");

  if (toggleButton) {
    toggleButton.addEventListener("click", _togglePassword);
  }
});

// TOGGLE PASSWORD VISIBILITY
function _togglePassword() {
  const passwordInput = document.getElementById("password");

  const toggleButton = document.getElementById("toggleLoginPassword");

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

// CLEAR VALIDATION
function clearValidation(element) {
  if (!element) {
    return;
  }

  element.classList.remove("is-invalid", "is-valid");
}

// CLEAR VALIDATION WHILE TYPING
document.addEventListener("DOMContentLoaded", () => {
  const emailInput = document.getElementById("email");

  const passwordInput = document.getElementById("password");

  if (emailInput) {
    emailInput.addEventListener("input", () => clearValidation(emailInput));
  }

  if (passwordInput) {
    passwordInput.addEventListener("input", () =>
      clearValidation(passwordInput),
    );
  }
});
