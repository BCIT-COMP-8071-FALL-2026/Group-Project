
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
document.addEventListener("DOMContentLoaded", function () {
	const personalWrapper = document.querySelector(".personal-detail-wrapper");

	if (!personalWrapper) return;

	const keepEditingSenior =
		personalWrapper.dataset.keepEditingSeniorInformation === "true";

	const keepEditingPersonal =
		personalWrapper.dataset.keepEditingPersonalInformation === "true";

	if (keepEditingPersonal) {
		const form = document.getElementById("personalDetailForm");

		if (form) {
			form.querySelectorAll("input:not([type='hidden']), textarea")
				.forEach(field => { field.disabled = false });
		}

		document.getElementById("personalDetailsActionBtn")
			?.classList.remove("d-none");

		document.getElementById("editPersonalDetailBtn")
			?.classList.add("d-none");
	}

	if (keepEditingSenior) {
		const form = document.getElementById("seniorDetailForm");

		if (form) {
			form.querySelectorAll("input:not([type='hidden']), textarea")
				.forEach(field => { field.disabled = false });
		}

		document.getElementById("seniorDetailsActionBtn")
			?.classList.remove("d-none");

		document.getElementById("editSeniorDetailBtn")
			?.classList.add("d-none");
	}
});