//document.addEventListener('DOMContentLoaded', () => {

//    let reservedDates = [];
//    let currentDate = new Date();
//    const today = new Date(new Date().setHours(0,0,0,0));
//    let confirmedRanges = [];

//    /* =========================
//       HELPERS
//    ========================= */

//    function formatDate(d){
//        return d.getFullYear() + '-' +
//            String(d.getMonth()+1).padStart(2,'0') + '-' +
//            String(d.getDate()).padStart(2,'0');
//    }

//    function getPreviousSaturday(date){
//        let d = new Date(date);
//        while(d.getDay() !== 6) d.setDate(d.getDate() - 1);
//        return d;
//    }

//    function removeHover(){
//        document.querySelectorAll('.hover-selected,.range-start,.range-end')
//            .forEach(c => c.classList.remove('hover-selected','range-start','range-end'));
//    }

//    function isRangeFree(start, end){
//        if(start < today) return false;

//        for(let d = new Date(start); d <= end; d.setDate(d.getDate()+1)){
//            if(reservedDates.includes(formatDate(d))) return false;
//        }
//        return true;
//    }

//    /* =========================
//       API
//    ========================= */

//    async function loadReservedDates(start, end){
//        try{
//            const res = await fetch(`/api/calendar/reserved?start=${start.toISOString()}&end=${end.toISOString()}`);
//            if(!res.ok) throw new Error();
//            reservedDates = await res.json();
//        }catch{
//            reservedDates = [];
//        }
//    }

//    /* =========================
//       RENDER CALENDAR
//    ========================= */

//    async function renderCalendar(){
//        const fetchStart = new Date(currentDate.getFullYear(), currentDate.getMonth()-1, 1);
//        const fetchEnd   = new Date(currentDate.getFullYear(), currentDate.getMonth()+2, 0);

//        await loadReservedDates(fetchStart, fetchEnd);

//        const wrapper = document.getElementById('calendarWrapper');
//        wrapper.innerHTML = '';

//        for(let i=0;i<2;i++){
//            const mDate = new Date(currentDate.getFullYear(), currentDate.getMonth()+i, 1);
//            wrapper.appendChild(generateMonth(mDate));
//        }
//    }

//    function generateMonth(date){
//        const monthDiv = document.createElement('div');
//        monthDiv.className = 'col-6';

//        const monthName = date.toLocaleString('pl-PL',{month:'long', year:'numeric'});

//        const table = document.createElement('table');
//        table.className = 'table table-bordered text-center';

//        table.innerHTML = `
//            <thead>
//                <tr><th colspan="7">${monthName}</th></tr>
//                <tr>
//                    <th>Pn</th><th>Wt</th><th>Śr</th>
//                    <th>Cz</th><th>Pt</th><th>So</th><th>Nd</th>
//                </tr>
//            </thead>
//        `;

//        const tbody = document.createElement('tbody');

//        let firstDay = date.getDay();
//        firstDay = firstDay === 0 ? 7 : firstDay;

//        let row = document.createElement('tr');
//        for(let i=1;i<firstDay;i++) row.appendChild(document.createElement('td'));

//        const daysInMonth = new Date(date.getFullYear(), date.getMonth()+1,0).getDate();

//        for(let d=1; d<=daysInMonth; d++){
//            const cell = document.createElement('td');
//            const cellDate = new Date(date.getFullYear(), date.getMonth(), d);
//            const iso = formatDate(cellDate);

//            cell.textContent = d;
//            cell.dataset.date = iso;
//            cell.classList.add('day-cell');

//            if(reservedDates.includes(iso)) cell.classList.add('reserved');
//            if(cellDate < today) cell.classList.add('past-day');
//            if(confirmedRanges.flat().includes(iso)) cell.classList.add('selected');
//            if(cellDate.getDay() === 6 && !cell.classList.contains('reserved'))
//                cell.classList.add('saturday-split');

//            if(cell.classList.contains('reserved') || cell.classList.contains('past-day')){
//                cell.classList.add('disabled');
//            }else{
//                cell.addEventListener('mouseenter', () => highlightWeek(cellDate));
//                cell.addEventListener('mouseleave', removeHover);
//                cell.addEventListener('click', () => confirmWeek(cellDate));
//            }

//            row.appendChild(cell);

//            if((firstDay + d - 1) % 7 === 0){
//                tbody.appendChild(row);
//                row = document.createElement('tr');
//            }
//        }

//        tbody.appendChild(row);
//        table.appendChild(tbody);
//        monthDiv.appendChild(table);

//        return monthDiv;
//    }

//    /* =========================
//       INTERACTIONS
//    ========================= */

//    function highlightWeek(date){
//        removeHover();

//        const weeks = parseInt(document.getElementById('weeksSelect').value);
//        let start = (date.getDay() === 6) ? new Date(date) : getPreviousSaturday(date);
//        let end = new Date(start);
//        end.setDate(end.getDate() + weeks * 7 - 1);

//        if(!isRangeFree(start, end)){
//            const info = document.getElementById('selectedRange');
//            info.innerHTML = `<div class="alert alert-warning">
//                Wybrany termin koliduje z inną rezerwacją
//            </div>`;
//            info.classList.remove('d-none');
//            return;
//        }

//        let first = true;
//        let lastCell = null;

//        for(let d = new Date(start); d <= end; d.setDate(d.getDate()+1)){
//            const cell = document.querySelector(`[data-date='${formatDate(d)}']`);
//            if(cell){
//                cell.classList.add('hover-selected');
//                if(first){
//                    cell.classList.add('range-start');
//                    first = false;
//                }
//                lastCell = cell;
//            }
//        }
//        if(lastCell) lastCell.classList.add('range-end');
//    }

//    function confirmWeek(date){
//        const weeks = parseInt(document.getElementById('weeksSelect').value);
//        let start = (date.getDay() === 6) ? new Date(date) : getPreviousSaturday(date);
//        let end = new Date(start);
//        end.setDate(end.getDate() + weeks * 7 - 1);

//        fetch('/api/calendar/calculate',{
//            method:'POST',
//            headers:{'Content-Type':'application/json'},
//            body: JSON.stringify({
//                from: start.toISOString(),
//                to: end.toISOString()
//            })
//        })
//        .then(res=>{
//            if(res.status === 409){
//                alert('❌ Termin już zajęty');
//                throw 'collision';
//            }
//            if(!res.ok){
//                alert('❌ Błąd obliczania ceny');
//                throw 'error';
//            }
//            return res.json();
//        })
//        .then(showReservationInfo)
//        .catch(()=>{});
//    }

//    /* =========================
//       RESERVATION INFO
//    ========================= */

//    function showReservationInfo(data){
//        const info = document.getElementById('selectedRange');

//        info.innerHTML = `
//            <p><strong>Termin:</strong> ${data.start} – ${data.end}</p>
//            <p><strong>Ilość dni:</strong> ${data.days}</p>
//            <p><strong>Cena:</strong> ${data.price} PLN</p>

//            <div class="form-check">
//                <input class="form-check-input" type="checkbox" id="c1">
//                <label class="form-check-label">Mam ukończone 18 lat</label>
//            </div>
//            <div class="form-check">
//                <input class="form-check-input" type="checkbox" id="c2">
//                <label class="form-check-label">Akceptuję regulamin</label>
//            </div>
//            <div class="form-check">
//                <input class="form-check-input" type="checkbox" id="c3">
//                <label class="form-check-label">Dane są prawdziwe</label>
//            </div>

//            <button id="btnContinue" class="btn btn-primary mt-3" disabled>
//                Kontynuuj rezerwację
//            </button>
//        `;

//        info.classList.remove('d-none');

//        const btn = document.getElementById('btnContinue');
//        const checks = ['c1','c2','c3'].map(id => document.getElementById(id));

//        checks.forEach(c =>
//            c.addEventListener('change', () => {
//                btn.disabled = !checks.every(x => x.checked);
//            })
//        );

//        btn.onclick = () => {
//            fetch('/GetCalendar/CreateNewReservation',{
//                method:'POST',
//                headers:{'Content-Type':'application/json'},
//                body: JSON.stringify({ from:data.start, to:data.end })
//            })
//            .then(r=>r.json())
//            .then(resp=>{
//                if(resp.success)
//                    window.location.href = resp.redirectUrl;
//            });
//        };
//    }

//    /* =========================
//       NAVIGATION
//    ========================= */

//    document.getElementById('nextBtn').onclick = () => {
//        currentDate = new Date(currentDate.getFullYear(), currentDate.getMonth()+2, 1);
//        renderCalendar();
//    };

//    document.getElementById('prevBtn').onclick = () => {
//        currentDate = new Date(currentDate.getFullYear(), currentDate.getMonth()-2, 1);
//        renderCalendar();
//    };

//    renderCalendar();

//});
















document.addEventListener('DOMContentLoaded', () => {

    let reservedDates = [];
    let currentDate = new Date();
    const today = new Date(new Date().setHours(0, 0, 0, 0));
    let confirmedRanges = [];

    /* ================== HELPERS ================== */

    function formatDate(d) {
        return d.getFullYear() + '-' +
            String(d.getMonth() + 1).padStart(2, '0') + '-' +
            String(d.getDate()).padStart(2, '0');
    }

    function getPreviousSaturday(date) {
        let d = new Date(date);
        while (d.getDay() !== 6) d.setDate(d.getDate() - 1);
        return d;
    }

    function isRangeFree(start, end) {
        if (start < today) return false;

        for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
            if (reservedDates.includes(formatDate(d))) return false;
        }
        return true;
    }

    /* ================== API ================== */

    async function loadReservedDates(start, end) {
        try {
            const response = await fetch(
                `/api/calendar/reserved?start=${start.toISOString()}&end=${end.toISOString()}`
            );
            if (!response.ok) throw new Error('Błąd pobierania zajętych dni');
            reservedDates = await response.json();
        } catch (err) {
            console.error(err);
            reservedDates = [];
        }
    }

    /* ================== CALENDAR RENDER ================== */

    async function renderCalendar() {

        const fetchStart = new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1);
        const fetchEnd = new Date(currentDate.getFullYear(), currentDate.getMonth() + 2, 0);
        await loadReservedDates(fetchStart, fetchEnd);

        const wrapper = document.getElementById('calendarWrapper');
        wrapper.innerHTML = '';

        for (let m = 0; m < 2; m++) {
            const monthDate = new Date(
                currentDate.getFullYear(),
                currentDate.getMonth() + m,
                1
            );
            wrapper.appendChild(generateMonth(monthDate));
        }
    }

    function generateMonth(date) {

        const monthDiv = document.createElement('div');
        monthDiv.className = 'col-12 col-md-6';   //zmiana

        const monthName = date.toLocaleString('pl-PL', {
            month: 'long',
            year: 'numeric'
        });

        const table = document.createElement('table');
        table.className = 'table table-bordered text-center';

        table.innerHTML = `
            <thead>
                <tr><th colspan="7">${monthName}</th></tr>
                <tr>
                    <th>Pn</th><th>Wt</th><th>Śr</th>
                    <th>Cz</th><th>Pt</th><th>So</th><th>Nd</th>
                </tr>
            </thead>
        `;

        const tbody = document.createElement('tbody');

        let firstDay = date.getDay();
        firstDay = firstDay === 0 ? 7 : firstDay;

        let row = document.createElement('tr');
        for (let i = 1; i < firstDay; i++)
            row.appendChild(document.createElement('td'));

        const daysInMonth = new Date(
            date.getFullYear(),
            date.getMonth() + 1,
            0
        ).getDate();

        for (let d = 1; d <= daysInMonth; d++) {

            const cell = document.createElement('td');
            const cellDate = new Date(date.getFullYear(), date.getMonth(), d);
            const iso = formatDate(cellDate);

            cell.textContent = d;
            cell.dataset.date = iso;
            cell.classList.add('day-cell');

            if (reservedDates.includes(iso)) cell.classList.add('reserved');
            if (cellDate < today) cell.classList.add('past-day');
            if (confirmedRanges.flat().includes(iso)) cell.classList.add('selected');
            if (cellDate.getDay() === 6 && !cell.classList.contains('reserved'))
                cell.classList.add('saturday-split');

            cell.addEventListener('mouseenter', () => highlightWeek(cellDate));
            cell.addEventListener('mouseleave', removeHover);
            cell.addEventListener('click', () => confirmWeek(cellDate));

            row.appendChild(cell);

            if ((firstDay + d - 1) % 7 === 0) {
                tbody.appendChild(row);
                row = document.createElement('tr');
            }
        }

        tbody.appendChild(row);
        table.appendChild(tbody);
        monthDiv.appendChild(table);

        return monthDiv;
    }

    /* ================== HOVER ================== */

    function highlightWeek(date) {

        removeHover();

        const weeks = parseInt(document.getElementById('weeksSelect').value);
        let start = (date.getDay() === 6) ? new Date(date) : getPreviousSaturday(date);
        let end = new Date(start);
        end.setDate(end.getDate() + weeks * 7 - 1);

        if (!isRangeFree(start, end)) return;

        for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
            const cell = document.querySelector(`[data-date='${formatDate(d)}']`);
            if (cell && !cell.classList.contains('selected'))
                cell.classList.add('hover-selected');
        }
    }

    function removeHover() {
        document
            .querySelectorAll('.hover-selected')
            .forEach(c => c.classList.remove('hover-selected'));
    }

    /* ================== SELECTION ================== */

    function markSelectedRange(start, end) {

        document
            .querySelectorAll('.day-cell.selected')
            .forEach(c => c.classList.remove('selected'));

        for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
            const cell = document.querySelector(
                `[data-date='${formatDate(d)}']`
            );
            if (cell) cell.classList.add('selected');
        }
    }

    function confirmWeek(date) {

        const weeks = parseInt(document.getElementById('weeksSelect').value);
        let start = (date.getDay() === 6) ? new Date(date) : getPreviousSaturday(date);
        let end = new Date(start);
        end.setDate(end.getDate() + weeks * 7 - 1);

        if (!isRangeFree(start, end)) {
            alert('❌ Wybrany termin jest niedostępny');
            return;
        }

        fetch('/api/calendar/calculate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                from: formatDate(start),
                to: formatDate(end)
            })
        })
            .then(res => {
                if (res.status === 409) {
                    alert('❌ Wybrany termin jest już zajęty');
                    throw 'collision';
                }
                if (!res.ok) {
                    alert('❌ Błąd obliczania ceny');
                    throw 'api error';
                }
                return res.json();
            })
            .then(data => {
                markSelectedRange(start, end);
                showReservationInfo(data);
            })
            .catch(() => { });
    }

    /* ================== INFO PANEL ================== */

    function showReservationInfo(data) {

        const info = document.getElementById('selectedRange');

        info.innerHTML = `
            <p><strong>Termin:</strong> ${data.start} – ${data.end}</p>
            <p><strong>Ilość dni:</strong> ${data.days}</p>
            <p><strong>Cena:</strong> ${data.price} PLN</p>

            <div class="form-check">
                <input class="form-check-input" type="checkbox" id="c1">
                <label class="form-check-label">Mam ukończone 18 lat</label>
            </div>
            <div class="form-check">
                <input class="form-check-input" type="checkbox" id="c2">
                <label class="form-check-label">Akceptuję regulamin i politykę prywatności</label>
            </div>
            <div class="form-check">
                <input class="form-check-input" type="checkbox" id="c3">
                <label class="form-check-label">Podane dane są prawdziwe</label>
            </div>

            <button id="btnContinue" class="btn btn-primary mt-3" disabled>
                Kontynuuj rezerwację
            </button>
        `;

        info.classList.remove('d-none');

        const btn = document.getElementById('btnContinue');
        const checks = ['c1', 'c2', 'c3'].map(id => document.getElementById(id));

        checks.forEach(c =>
            c.addEventListener('change', () => {
                btn.disabled = !checks.every(x => x.checked);
            })
        );

        btn.onclick = () => {
            fetch('/GetCalendar/CreateNewReservation', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    from: formatDate(start),
                    to: formatDate(end)
                })
            })
                .then(r => r.json())
                .then(resp => {
                    if (resp.success)
                        window.location.href = resp.redirectUrl;
                });
        };
    }

    /* ================== NAVIGATION ================== */

    document.getElementById('nextBtn').onclick = () => {
        currentDate = new Date(currentDate.getFullYear(), currentDate.getMonth() + 2, 1);
        renderCalendar();
    };

    document.getElementById('prevBtn').onclick = () => {
        currentDate = new Date(currentDate.getFullYear(), currentDate.getMonth() - 2, 1);
        renderCalendar();
    };

    renderCalendar();
});








//document.addEventListener('DOMContentLoaded', () => {




//let currentDate = new Date();
//currentDate.setDate(1);

//let selectedStart = null;
//let selectedEnd = null;

//const calendarWrapper = document.getElementById("calendarWrapper");
//const selectedRangeDiv = document.getElementById("selectedRange");
//const weeksSelect = document.getElementById("weeksSelect");

//document.getElementById("prevBtn").addEventListener("click", () => {
//    currentDate.setMonth(currentDate.getMonth() - 2);
//    renderCalendars();
//});

//document.getElementById("nextBtn").addEventListener("click", () => {
//    currentDate.setMonth(currentDate.getMonth() + 2);
//    renderCalendars();
//});

//weeksSelect.addEventListener("change", () => {
//    if (selectedStart) {
//        selectRange(selectedStart);
//    }
//});

//function renderCalendars() {
//    calendarWrapper.innerHTML = "";

//    for (let i = 0; i < 2; i++) {
//        const date = new Date(currentDate.getFullYear(), currentDate.getMonth() + i, 1);
//        calendarWrapper.appendChild(renderSingleCalendar(date));
//    }

//    highlightSelection();
//}

//function renderSingleCalendar(date) {
//    const monthDiv = document.createElement("div");
//    monthDiv.className = "col-md-6";

//    const title = document.createElement("h5");
//    title.className = "text-center";
//    title.innerText = date.toLocaleDateString("pl-PL", {
//        month: "long",
//        year: "numeric"
//    });

//    const table = document.createElement("table");
//    table.className = "calendar-table";

//    const thead = document.createElement("thead");
//    thead.innerHTML = `<tr>
//        <th>PN</th><th>WT</th><th>ŚR</th><th>CZ</th>
//        <th>PT</th><th>SO</th><th>ND</th>
//    </tr>`;
//    table.appendChild(thead);

//    const tbody = document.createElement("tbody");

//    let firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
//    let startDay = (firstDay.getDay() + 6) % 7;

//    let daysInMonth = new Date(date.getFullYear(), date.getMonth() + 1, 0).getDate();

//    let row = document.createElement("tr");
//    for (let i = 0; i < startDay; i++) {
//        row.appendChild(document.createElement("td"));
//    }

//    for (let day = 1; day <= daysInMonth; day++) {
//        if (row.children.length === 7) {
//            tbody.appendChild(row);
//            row = document.createElement("tr");
//        }

//        const cell = document.createElement("td");
//        const cellDate = new Date(date.getFullYear(), date.getMonth(), day);

//        cell.innerText = day;
//        cell.dataset.date = formatDate(cellDate);

//        if (cellDate.getDay() === 6) {
//            cell.classList.add("saturday");
//        }

//        cell.addEventListener("click", () => {
//            if (cellDate.getDay() !== 6) return;
//            selectRange(cellDate);
//        });

//        row.appendChild(cell);
//    }

//    tbody.appendChild(row);
//    table.appendChild(tbody);

//    monthDiv.appendChild(title);
//    monthDiv.appendChild(table);

//    return monthDiv;
//}

//function selectRange(startDate) {
//    const weeks = parseInt(weeksSelect.value);
//    selectedStart = new Date(startDate.getFullYear(), startDate.getMonth(), startDate.getDate());
//    selectedEnd = new Date(selectedStart);
//    selectedEnd.setDate(selectedStart.getDate() + weeks * 7 - 1);

//    highlightSelection();
//    showSelectedRange();
//}

//function highlightSelection() {
//    document.querySelectorAll(".calendar-table td").forEach(td => {
//        td.classList.remove("selected");
//    });

//    if (!selectedStart || !selectedEnd) return;

//    document.querySelectorAll(".calendar-table td").forEach(td => {
//        if (!td.dataset.date) return;

//        const d = parseDate(td.dataset.date);
//        if (d >= selectedStart && d <= selectedEnd) {
//            td.classList.add("selected");
//        }
//    });
//}

//function showSelectedRange() {
//    selectedRangeDiv.classList.remove("d-none");
//    selectedRangeDiv.innerHTML =
//        `<strong>Wybrany termin:</strong>
//        ${formatDatePL(selectedStart)} – ${formatDatePL(selectedEnd)}`;
//}

///* ======= DATY – BEZ UTC ======= */

//function formatDate(date) {
//    const y = date.getFullYear();
//    const m = String(date.getMonth() + 1).padStart(2, "0");
//    const d = String(date.getDate()).padStart(2, "0");
//    return `${y}-${m}-${d}`;
//}

//function parseDate(str) {
//    const [y, m, d] = str.split("-").map(Number);
//    return new Date(y, m - 1, d);
//}

//function formatDatePL(date) {
//    return date.toLocaleDateString("pl-PL", {
//        day: "numeric",
//        month: "long",
//        year: "numeric"
//    });
//}

//renderCalendars();

//});