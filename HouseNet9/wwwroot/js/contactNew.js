function removeElement(button) {
    const item = button.closest(".removable-item");

    if (item) {
        item.remove();
    }
}

// ==========================
// PHONES
// ==========================
function addPhone() {
    const container = document.getElementById("phones");
    const key = crypto.randomUUID();

    container.insertAdjacentHTML("beforeend", `
        <div class="removable-item mb-2">

            <input type="hidden"
                   name="PhoneNumbers.Index"
                   value="${key}" />

            <input type="hidden"
                   name="PhoneNumbers[${key}].PhoneNumberId"
                   value="0" />

            <input type="text"
                   name="PhoneNumbers[${key}].Number"
                   class="form-control d-inline w-75" />

            <button type="button"
                    class="btn btn-sm btn-danger"
                    onclick="removeElement(this)">
                Usuń
            </button>

        </div>
    `);
}

// ==========================
// EMAILS
// ==========================
function addEmail() {
    const container = document.getElementById("emails");
    const key = crypto.randomUUID();

    container.insertAdjacentHTML("beforeend", `
        <div class="removable-item mb-2">

            <input type="hidden"
                   name="EmailAddresses.Index"
                   value="${key}" />

            <input type="hidden"
                   name="EmailAddresses[${key}].EmailAddressId"
                   value="0" />

            <input type="email"
                   name="EmailAddresses[${key}].Email"
                   class="form-control d-inline w-75" />

            <button type="button"
                    class="btn btn-sm btn-danger"
                    onclick="removeElement(this)">
                Usuń
            </button>

        </div>
    `);
}

// ==========================
// ADDRESSES
// ==========================
function addAddress() {
    const container = document.getElementById("addresses");
    const key = crypto.randomUUID();

    container.insertAdjacentHTML("beforeend", `
        <div class="removable-item mb-2">

            <input type="hidden"
                   name="Addresses.Index"
                   value="${key}" />

            <input type="hidden"
                   name="Addresses[${key}].AddressId"
                   value="0" />

            <input type="text"
                   name="Addresses[${key}].Street"
                   class="form-control mb-1" />

            <input type="text"
                   name="Addresses[${key}].PostalCode"
                   class="form-control mb-1" />

            <input type="text"
                   name="Addresses[${key}].City"
                   class="form-control mb-1" />

            <input type="text"
                   name="Addresses[${key}].Country"
                   class="form-control mb-1" />

            <button type="button"
                    class="btn btn-sm btn-danger"
                    onclick="removeElement(this)">
                Usuń
            </button>

        </div>
    `);
}