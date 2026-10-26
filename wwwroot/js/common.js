document.addEventListener("DOMContentLoaded", () => {});

function esc(v) {
	return v == null
		? ""
		: String(v)
				.replaceAll("&", "&amp;")
				.replaceAll("<", "&lt;")
				.replaceAll(">", "&gt;")
				.replaceAll('"', "&quot;")
				.replaceAll("'", "&#039;");
}

function money(v) {
	return Number(v || 0).toLocaleString("en-IN", {
		style: "currency",
		currency: "INR",
		maximumFractionDigits: 2,
	});
}

function dateOnly(v) {
	if (!v) return "-";
	const d = new Date(v);
	return isNaN(d)
		? "-"
		: d.toLocaleDateString("en-IN", {
				day: "2-digit",
				month: "short",
				year: "numeric",
			});
}

function dateTime(v) {
	if (!v) return "-";
	const d = new Date(v);
	return isNaN(d)
		? "-"
		: d.toLocaleString("en-IN", {
				day: "2-digit",
				month: "short",
				year: "numeric",
				hour: "2-digit",
				minute: "2-digit",
			});
}

function role() {
	return localStorage.getItem("userRole") || "";
}

function isManagement() {
	return ["Admin", "Service Advisor"].includes(role());
}

function alertBox(id, msg, type = "success") {
	const e = document.getElementById(id);
	if (!e) return;
	e.className = `alert alert-${type}`;
	e.textContent = msg;
	e.classList.remove("d-none");
	setTimeout(() => e.classList.add("d-none"), 4000);
}

function setBusy(btn, busy, text = "Saving...") {
	btn.disabled = busy;
	if (busy) {
		btn.dataset.oldText = btn.innerHTML;
		btn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span>${text}`;
	} else btn.innerHTML = btn.dataset.oldText || text;
}
