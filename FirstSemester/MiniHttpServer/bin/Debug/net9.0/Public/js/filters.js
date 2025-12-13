document.addEventListener("DOMContentLoaded", () => {
    const checkboxes = document.querySelectorAll('.filter-checkbox');

    // 1. Сначала восстанавливаем состояние из URL (ставим галочки)
    restoreFiltersFromUrl();

    // 2. Если мы восстановили какие-то галочки, нужно сразу применить фильтр
    if (document.querySelectorAll('.filter-checkbox:checked').length > 0) {
        sendFilterRequest();
    }

    // 3. Вешаем обработчики событий
    checkboxes.forEach(box => {
        box.addEventListener('change', () => {
            updateUrlFromFilters(); // Обновляем URL
            sendFilterRequest();    // Шлем запрос на сервер
        });
    });
});

/**
 * Собирает данные с чекбоксов и отправляет POST запрос на сервер
 */
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

/**
 * Читает текущие галочки и обновляет URL браузера
 * (без перезагрузки страницы)
 */
function updateUrlFromFilters() {
    const params = new URLSearchParams();

    document.querySelectorAll('.filter-checkbox:checked').forEach(box => {
        const category = box.getAttribute('data-category'); // 'days', 'months'...
        const value = box.value;
        // Добавляем параметр, например: days=3
        params.append(category, value);
    });

    // Формируем новую строку адреса
    // window.location.pathname - это текущий путь (например, /tours/list)
    const newUrl = `${window.location.pathname}?${params.toString()}`;

    // Меняем URL в истории браузера
    window.history.replaceState({}, '', newUrl);
}

/**
 * При загрузке страницы читает URL и проставляет галочки
 */
function restoreFiltersFromUrl() {
    // Получаем параметры из URL (все, что после ?)
    const params = new URLSearchParams(window.location.search);

    // Бежим по всем возможным чекбоксам на странице
    document.querySelectorAll('.filter-checkbox').forEach(box => {
        const category = box.getAttribute('data-category');
        const value = box.value;

        // Проверяем, есть ли такое значение в URL
        // Например, если URL ?days=3&days=7, то params.getAll('days') вернет ["3", "7"]
        const valuesInUrl = params.getAll(category);

        if (valuesInUrl.includes(value)) {
            box.checked = true;
        }
    });
}