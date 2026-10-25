
const editPersonalDetailBtn = document.getElementById("editPersonalDetailBtn");
const editSeniorDetailBtn = document.getElementById("editSeniorDetailBtn");
const seniorDetailsActionBtn = document.getElementById("seniorDetailsActionBtn");
const personalDetailsActionBtn = document.getElementById("personalDetailsActionBtn");
const personalDetailForm = document.querySelector("#personalDetailForm");
const phoneInput = document.getElementById("phoneInput");


const addressInputDetail = document.getElementById("addressInputDetail");
const addressInput = document.getElementById("addressInput");
const isLivingAloneData = document.getElementById("isLivingAloneData");
const isLivingAloneInput = document.getElementById("isLivingAloneInput");
const seniorDetailForm = document.querySelector("#seniorDetailForm");

setFormEditable(personalDetailForm, false);
setFormEditable(seniorDetailForm, false);

editPersonalDetailBtn.addEventListener("click", function() {
	personaltoggle();
setFormEditable(personalDetailForm, true);

});

editSeniorDetailBtn.addEventListener("click", function() {
	seniorToggle();
setFormEditable(seniorDetailForm, true);

})

phoneInput.addEventListener("input", function () {
	const phoneInput = document.getElementById("phoneInput1");

	if (!phoneInput) return;

	const iti = window.intlTelInput(phoneInput, {
		initialCountry: "ca",
		separateDialCode: true,
		nationalMode: true,
		loadUtils: () => import(
			"https://cdn.jsdelivr.net/npm/intl-tel-input@25.10.1/build/js/utils.js"
		)
	});

	phoneInput.form.addEventListener("submit", function () {
		phoneInput.value = iti.getNumber();
	});
});

function cancelEditPersonalInfo() {
	personalDetailForm.querySelectorAll("input").forEach(input => {
		input.classList.remove("edited");
	});
	personaltoggle();
	setFormEditable(personalDetailForm, false);

}

function cancelEditSeniorInfo() {
	seniorDetailForm.querySelectorAll("input").forEach(input => {
		input.classList.remove("edited");
	});
seniorToggle();
setFormEditable(seniorDetailForm, false);
}

function personaltoggle() {
	addressInputDetail.classList.toggle("d-none");
	addressInput.classList.toggle("d-none");
	personalDetailsActionBtn.classList.toggle("d-none");
	editPersonalDetailBtn.classList.toggle("d-none");
	editSeniorDetailBtn.classList.toggle("d-none");
}

function seniorToggle() {
	isLivingAloneData.classList.toggle("d-none");
	isLivingAloneInput.classList.toggle("d-none");
	seniorDetailsActionBtn.classList.toggle("d-none");
	editSeniorDetailBtn.classList.toggle("d-none");
	editPersonalDetailBtn.classList.toggle("d-none");
}

function setFormEditable(form, editable) {
	form.querySelectorAll("input:not([type='hidden'])").forEach(input => {
		if (input.type === "radio" || input.type === "checkbox") {
			input.disabled = !editable;
		} else {
			input.disabled = !editable;
		}
	});
}

function applyInitialNameColors() {
	document.querySelectorAll(".initial-name").forEach(function (element) {
		const name = element.dataset.name || "";
		let hash = 0;

		for (let i = 0; i < name.length; i++) {
			hash = name.charCodeAt(i) + ((hash << 5) - hash);
		}

		const hue = Math.abs(hash % 360);

		element.style.setProperty(
			"background-color",
			`hsl(${hue}, 60%, 45%)`,
			"important"
		);
		element.style.color = "#ffffff";
	});
}

// Run on initial page load
document.addEventListener("DOMContentLoaded", applyInitialNameColors);