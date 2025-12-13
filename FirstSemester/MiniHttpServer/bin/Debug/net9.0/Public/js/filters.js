document.addEventListener("DOMContentLoaded", () => {
    const checkboxes = document.querySelectorAll('.filter-checkbox');

    restoreFiltersFromUrl();

    if (document.querySelectorAll('.filter-checkbox:checked').length > 0) {
        sendFilterRequest();
    }

    checkboxes.forEach(box => {
        box.addEventListener('change', () => {
            updateUrlFromFilters(); 
            sendFilterRequest();    
        });
    });
});

function sendFilterRequest() {
    const filters = {
        months: [],
        seasons: [],
        types: [],
        days: []
    };

    document.querySelectorAll('.filter-checkbox:checked').forEach(box => {
        const category = box.getAttribute('data-category');
        const value = box.value;

        if (filters[category]) {
            filters[category].push(value);
        }
    });

    fetch('/tours/filter', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(filters)
    })
        .then(response => {
            if (!response.ok) throw new Error("Server error");
            return response.text();
        })
        .then(htmlFragment => {
            const container = document.getElementById('tours-container');
            if(container) {
                container.innerHTML = htmlFragment;
            }
        })
        .catch(error => console.error('Error:', error));
}

function updateUrlFromFilters() {
    const params = new URLSearchParams();

    document.querySelectorAll('.filter-checkbox:checked').forEach(box => {
        const category = box.getAttribute('data-category'); 
        const value = box.value;
        params.append(category, value);
    });

    const newUrl = `${window.location.pathname}?${params.toString()}`;

    window.history.replaceState({}, '', newUrl);
}

function restoreFiltersFromUrl() {
    const params = new URLSearchParams(window.location.search);

    document.querySelectorAll('.filter-checkbox').forEach(box => {
        const category = box.getAttribute('data-category');
        const value = box.value;

        const valuesInUrl = params.getAll(category);

        if (valuesInUrl.includes(value)) {
            box.checked = true;
        }
    });
}