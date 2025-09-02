
    function addPhone() {
            const container = document.getElementById("phoneNumbers");
    const index = container.querySelectorAll(".phone-item").length;

    const div = document.createElement("div");
    div.className = "mb-3 phone-item";
    div.innerHTML = `
    <label class="form-label">Phone Number</label>
    <input type="text" name="PhoneNumbers[${index}].Number" class="form-control" placeholder="Enter phone number">
        `;
        container.appendChild(div);
        }

        function addEmail() {
            const container = document.getElementById("emails");
        const index = container.querySelectorAll(".email-item").length;

        const div = document.createElement("div");
        div.className = "mb-3 email-item";
        div.innerHTML = `
        <label class="form-label">Email</label>
        <input type="email" name="EmailAddresses[${index}].Email" class="form-control" placeholder="Enter email">
            `;
            container.appendChild(div);
        }

            function addAddress() {
            const container = document.getElementById("addresses");
            const index = container.querySelectorAll(".address-card").length;

            const card = document.createElement("div");
            card.className = "card mb-3 address-card";
            card.innerHTML = `
            <div class="card-body">
                <h5 class="card-title">Address ${index + 1}</h5>
                <div class="mb-2">
                    <label class="form-label">Street</label>
                    <input type="text" name="Addresses[${index}].Street" class="form-control" placeholder="Street">
                </div>
                <div class="mb-2">
                    <label class="form-label">City</label>
                    <input type="text" name="Addresses[${index}].City" class="form-control" placeholder="City">
                </div>
                <div class="mb-2">
                    <label class="form-label">Postal Code</label>
                    <input type="text" name="Addresses[${index}].PostalCode" class="form-control" placeholder="00-000">
                </div>
                <div class="mb-2">
                    <label class="form-label">Country</label>
                    <input type="text" name="Addresses[${index}].Country" class="form-control" placeholder="Country">
                </div>
            </div>
            `;
            container.appendChild(card);
        }
