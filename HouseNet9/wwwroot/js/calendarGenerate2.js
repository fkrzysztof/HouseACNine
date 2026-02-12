document.addEventListener('DOMContentLoaded', () => {

    let reservedDates = [];
    let currentDate = new Date();
    const today = new Date(new Date().setHours(0, 0, 0, 0));
    let confirmedRanges = [];

    // globalne wybrane daty
    let selectedStart = null;
    let selectedEnd = null;

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

    async function loadReservedDates(start, end) {
        try {
            const res = await fetch(`/api/calendar/reserved?start=${start.toISOString()}&end=${end.toISOString()}`);
            if (!res.ok) throw new Error('Błąd pobierania zajętych dni');
            reservedDates = await res.json();
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
            const monthDate = new Date(currentDate.getFullYear(), currentDate.getMonth() + m, 1);
            wrapper.appendChild(generateMonth(monthDate));
        }
    }

    function generateMonth(date) {
        const monthDiv = document.createElement('div');
        monthDiv.className = 'col-12 col-md-6';

        const monthName = date.toLocaleString('pl-PL', { month: 'long', year: 'numeric' });

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

        const daysInMonth = new Date(date.getFullYear(), date.getMonth() + 1, 0).getDate();

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

            if (!cell.classList.contains('reserved') && !cell.classList.contains('past-day')) {
                cell.addEventListener('click', () => confirmWeek(cellDate));
                cell.addEventListener('mouseenter', () => highlightWeek(cellDate));
                cell.addEventListener('mouseleave', removeHover);
            }

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
        document.querySelectorAll('.hover-selected').forEach(c => c.classList.remove('hover-selected'));
    }

    /* ================== SELECTION ================== */

    function markSelectedRange(start, end) {
        document.querySelectorAll('.day-cell.selected').forEach(c => c.classList.remove('selected'));
        for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
            const cell = document.querySelector(`[data-date='${formatDate(d)}']`);
            if (cell) cell.classList.add('selected');
        }
    }

    function confirmWeek(date) {
        const weeks = parseInt(document.getElementById('weeksSelect').value);
        selectedStart = (date.getDay() === 6) ? new Date(date) : getPreviousSaturday(date);
        selectedEnd = new Date(selectedStart);
        selectedEnd.setDate(selectedEnd.getDate() + weeks * 7 - 1);

        if (!isRangeFree(selectedStart, selectedEnd)) {
            alert('❌ Wybrany termin jest niedostępny');
            return;
        }

        fetch('/api/calendar/calculate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                from: formatDate(selectedStart),
                to: formatDate(selectedEnd)
            })
        })
            .then(res => {
                if (!res.ok) throw new Error('Błąd API');
                return res.json();
            })
            .then(data => {
                markSelectedRange(selectedStart, selectedEnd);
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

            <button id="btnContinue" class="btn btn-primary mt-3" disabled>Kontynuuj rezerwację</button>
        `;
        info.classList.remove('d-none');

        const btn = document.getElementById('btnContinue');
        const checks = ['c1', 'c2', 'c3'].map(id => document.getElementById(id));
        checks.forEach(c => c.addEventListener('change', () => {
            btn.disabled = !checks.every(x => x.checked);
        }));

        btn.onclick = () => {
            if (!selectedStart || !selectedEnd) {
                alert("Brak wybranego terminu");
                return;
            }

            fetch('/GetCalendar/CreateNewReservation', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    from: formatDate(selectedStart),
                    to: formatDate(selectedEnd)
                })
            })
                .then(r => {
                    if (!r.ok) throw new Error();
                    return r.json();
                })
                .then(resp => {
                    if (resp.redirectUrl) window.location.href = resp.redirectUrl;
                })
                .catch(() => alert("Termin zajęty"));
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
