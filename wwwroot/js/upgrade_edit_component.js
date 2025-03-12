let modal_edit = document.getElementsByClassName("modal_edit_comp")[0];
let modal_edit_content = document.getElementsByClassName("modal_edit_comp_content")[0];
let close_edit = document.getElementById("close_id_edit_comp");
let btn_send_edit = document.getElementById("send_data_edit");
let btn_edit = document.getElementById("edit_comp_bt");
let table = document.querySelector('#table_view');

const BASE_URL = window.location.origin;
const EDIT_URL = `${BASE_URL}/Home/EditComponent`;

// Открытие модального окна
let open_modal = () => {
    modal_edit.style.display = "block";
}

// Закрытие модального окна
let close_modal = () => {
    modal_edit_content.innerHTML = "";
    modal_edit.style.display = "none";
}

close_edit.addEventListener('click', close_modal);

// Проверка заполненности всех инпутов
const validateInputs = () => {
    const inputs = document.querySelectorAll(".edit_inputs");
    for (let input of inputs) {
        if (!input.value.trim()) {
            alert("Все поля должны быть заполнены.");
            return false;
        }
    }
    return true;
};

// Сбор данных из формы
const collectData = () => {
    const inputs = document.querySelectorAll(".edit_inputs");
    const labels = document.querySelectorAll(".edit_label_comp");
    let data = [];

    let currentRecord = {};
    inputs.forEach((input, index) => {
        const label = labels[index];
        if (label) {
            const key = label.textContent.replace(": ", "").trim(); // Убираем лишние символы
            const value = input.value.trim(); // Убираем пробелы

            // Если это начало новой записи (например, PartNumber)
            if (key === "PartNumber" && Object.keys(currentRecord).length > 0) {
                data.push(currentRecord); // Сохраняем текущую запись
                currentRecord = {}; // Начинаем новую запись
            }

            currentRecord[key] = value; // Добавляем поле в текущую запись
        }
    });

    // Добавляем последнюю запись, если она есть
    if (Object.keys(currentRecord).length > 0) {
        data.push(currentRecord);
    }

    return data;
};

// Отправка данных на сервер
const sendData = async (json) => {
    try {
        console.log("Отправляемые данные:", JSON.stringify(json, null, 2)); // Логирование JSON
        const response = await fetch(EDIT_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(json),
        });

        if (response.ok) {
            alert('Успешно обновлено!');
            close_modal();
        } else {
            const errorText = await response.text(); // Получаем текст ошибки от сервера
            console.error("Ошибка сервера:", errorText);
            alert('Произошла ошибка. Повторите попытку позже.');
        }
    } catch (error) {
        console.error('Ошибка:', error);
        alert('Произошла ошибка. Повторите попытку позже.');
    }
};

const handleSendEdit = () => {
    if (validateInputs()) {
        const data = collectData();
        const json = {
            table: table_name, // Имя таблицы, переданное в функцию edit_comp
            comps: data // Массив записей
        };
        console.log("Отправляемые данные:", JSON.stringify(json, null, 2)); // Логирование JSON
        sendData(json);
    }
};
const validateInputs = () => {
    const inputs = document.querySelectorAll(".edit_inputs");
    for (let input of inputs) {
        if (!input.value.trim()) {
            alert("Все поля должны быть заполнены.");
            return false;
        }
    }
    return true;
};
// Основная логика редактирования
export function edit_comp(table_name, data_json) {
    btn_edit.addEventListener('click', () => {
        open_modal();
        modal_edit_content.innerHTML = '';

        //let json = {
        //    "table": table_name,
        //    "comps": [] // Теперь это массив записей (изменить на объект {} если не получится реализовать логику)
        //};
        let json = {
            "table": table_name,
            "comp": {}
        };

        modal_edit_content.appendChild(close_edit);
        modal_edit_content.appendChild(btn_send_edit);

        let table_label = document.createElement('label');
        table_label.textContent = `Таблица "${table_name}"`;
        table_label.style.fontWeight = 'bold';
        table_label.style.fontSize = '22px';
        modal_edit_content.appendChild(table_label);

        let main_label = document.createElement('label');
        main_label.textContent = 'Список компонентов для редактирования: ';
        modal_edit_content.appendChild(main_label);

        data_json.forEach((object_json, recordIndex) => {
            let container = document.createElement('div');
            container.style.marginBottom = '20px';
            container.style.display = 'flex';
            container.style.flexDirection = 'column';
            container.style.gap = '10px';

            Object.keys(object_json).forEach((key) => {
                let pairContainer = document.createElement('div');
                pairContainer.style.display = 'flex';
                pairContainer.style.alignItems = 'center';
                pairContainer.style.gap = '10px';

                let label = document.createElement('label');
                label.classList.add("edit_label_comp");
                label.textContent = key + ": ";
                label.style.fontWeight = 'bold';
                label.style.minWidth = '100px';
                pairContainer.appendChild(label);

                let input = document.createElement('input');
                input.classList.add("edit_inputs");

                // Если это поле PartNumber, делаем его readonly
                if (key === "PartNumber") {
                    input.setAttribute('readonly', true); // Делаем поле неизменяемым
                    input.style.backgroundColor = '#f0f0f0'; // Меняем цвет фона для наглядности
                }

                input.value = object_json[key];
                input.style.flex = '1';
                pairContainer.appendChild(input);

                container.appendChild(pairContainer);
            });
            modal_edit_content.appendChild(container);
        });

        btn_send_edit.addEventListener('click', handleSendEdit);
    });
}
