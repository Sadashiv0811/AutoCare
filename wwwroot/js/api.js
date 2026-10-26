const API_BASE_URL = "/api";

async function apiRequest(endpoint, options = {}) {
	const token = localStorage.getItem("token");

	const headers = {
		"Content-Type": "application/json",
		...(options.headers || {}),
	};

	if (token) {
		headers["Authorization"] = `Bearer ${token}`;
	}

	try {
		const response = await fetch(`${API_BASE_URL}${endpoint}`, {
			...options,
			headers,
		});

		if (response.status === 401) {
			localStorage.removeItem("token");
			localStorage.removeItem("userEmail");
			window.location.replace("/pages/login.html");
			return null;
		}

		// 204 responses intentionally have no body.
		if (response.status === 204) {
			return null;
		}

		// Some authorization/server errors have an empty or non-JSON body.
		// Read text first so response.json() cannot throw "Unexpected end of JSON input".
		const responseText = await response.text();
		let data = null;

		if (responseText.trim()) {
			try {
				data = JSON.parse(responseText);
			} catch {
				data = null;
			}
		}

		if (!response.ok) {
			const message =
				data?.message ||
				data?.title ||
				(typeof responseText === "string" && responseText.trim()
					? responseText.trim().slice(0, 300)
					: `Request failed with HTTP ${response.status} (${response.statusText || "API error"}).`);
			throw new Error(message);
		}

		if (responseText.trim() && data === null) {
			throw new Error("The server returned an invalid response. Check the API logs for details.");
		}

		return data;
	} catch (error) {
		console.error("API Error:", error);
		throw error;
	}
}

// GET
async function apiGet(endpoint) {
	return await apiRequest(endpoint, {
		method: "GET",
	});
}

// POST
async function apiPost(endpoint, data) {
	return await apiRequest(endpoint, {
		method: "POST",
		body: JSON.stringify(data),
	});
}

// PUT
async function apiPut(endpoint, data) {
	return await apiRequest(endpoint, {
		method: "PUT",
		body: JSON.stringify(data),
	});
}

// DELETE
async function apiDelete(endpoint) {
	return await apiRequest(endpoint, {
		method: "DELETE",
	});
}
