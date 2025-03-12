//import { Tab } from "../lib/bootstrap/dist/js/bootstrap.bundle";

let btn_edit = document.getElementById("edit_comp_bt");
let btn_send_edit = document.createElement('button');
let input_edit = document.createElement('input');
btn_send_edit.innerText = 'Внести данные редактирования';
btn_send_edit.id = 'id_btn_send_edit';
input_edit.id = 'id_input_edit';
//let isEditMode = false; //флаг открытия окна
/*флаги режима редактирования*/
let cell_on = 1;
let cell_off = 0;
/*флаг модификации ячейки*/
let cell_modified = false;
/*структура json для отправки данных*/
let json = {
    table: '',
    comp: {}
};
/*включение режима редактирования*/
function cell_edit_enable(state) {
    if (state == 1) {
        document.querySelectorAll('td').forEach(td => {
            td.ondblclick = function () {
                td.contentEditable = true;
            }
        });
    }
    else if (state == 0) {
        document.querySelectorAll('td').forEach(td => {
            td.ondblclick = function () {
                td.contentEditable = false;
                alert('Режим редактирования ячейки закрыт');
            }
        });
    }
    else {
        alert('Некорректные условия закрытия режима редактирвоания');
    }

}

/*основная логика редактирования записей*/
export function edit_comp(table_name) {
    const table = document.querySelector("#table_view");
    table.querySelectorAll('thead').forEach(th => {
        console.log("Thead: " + JSON.stringify(th));
    });
    // Активация режима редактирования по нажатию кнопки
    btn_edit.addEventListener('click', (evt) => {
        let tb_btns = document.getElementById('tb_btns_id');
        tb_btns.appendChild(btn_send_edit);
        btn_send_edit.style.display = "block";
        alert('Режим редактирования активирован. Дважды кликните по ячейке, чтобы изменить её значение.');
        cell_edit_enable(cell_on);
        let td = table.querySelectorAll('td').forEach(td =>
        {
            td.addEventListener('click', (e) =>
            {
                if (editCell(td))
                {
                    td.bgColor = '#FFE4B5';
                }
            });
        });
    });
    btn_send_edit.addEventListener('click', (evt) => {
        let json = collectTableData(table, table_name);
        send_data(json);
    });
}
/*метод отправки данных*/
async function send_data(json)
{
    try {
        console.log("Отправляемые данные:", JSON.stringify(json, null, 2)); // Логирование JSON
        const response = await fetch('https://localhost:7205/Home/EditComponent', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(json),
        });

        if (response.ok) {
            alert('Успешно обновлено!');
            // Обновляем таблицу
            btn_send_edit.style.display = "none";
            tb_btns.removeChild(btn_send_edit);
            await loadTableData(json.table);
        } else {
            const errorText = await response.text(); // Получаем текст ошибки от сервера
            console.error("Ошибка сервера:", errorText);
            alert('Произошла ошибка. Повторите попытку позже.');
        }
    } catch (error) {
        console.error('Ошибка:', error);
        alert('Произошла ошибка. Повторите попытку позже.');
    }
}
/*запись данных с ячеек в json, возврат готового json*/
function collectTableData(table, tableName) {
    const headers = [];
    const comp = [];
    const headerRow = table.querySelector('thead tr');
    headerRow.querySelectorAll('td').forEach(td => {
        headers.push(td.textContent.trim().replace(':', '')); 
    });
    const bodyRows = table.querySelectorAll('tbody tr');
    bodyRows.forEach(tr => {
        const rowData = {};
        const cells = tr.querySelectorAll('td');
        //const primaryKey = cells[0].textContent.trim(); // Первый столбец — это PartNumber_name_1
        cells.forEach((td, index) => {
            //if (index > 0) { // Пропускаем первый столбец, так как он используется как ключ
                rowData[headers[index]] = td.textContent.trim();
            //}
        });

        //comp[primaryKey] = rowData; // Добавляем данные в объект comp
        comp.push(rowData);
    });
    json.table = tableName;
    json.comp = comp;


    return json;
}
/*метод активации режима редактирования return - true/false*/
function editCell(cell) {
    // Сохраняем текущее значение ячейки
    const originalValue = cell.textContent;

    // Создаем текстовое поле для редактирования
    const input = document.createElement('input');
    input.type = 'text';
    input.value = originalValue;

    // Очищаем ячейку и вставляем текстовое поле
    cell.textContent = '';
    cell.appendChild(input);

    // Фокусируемся на текстовом поле
    input.focus();

    // Обработка завершения редактирования
    input.addEventListener('blur', () => {
        finishEditing(cell, input);
    });

    input.addEventListener('keypress', (evt) => {
        if (evt.key === 'Enter') {
            finishEditing(cell, input);
        }
    });
    return true;
}
/*финал редактирования*/
function finishEditing(cell, input) {
    // Сохраняем новое значение
    const newValue = input.value;
    cell.textContent = newValue;

    // Здесь можно добавить логику для сохранения изменений в JSON или отправки на сервер
    console.log(`Ячейка изменена: ${newValue}`);
}