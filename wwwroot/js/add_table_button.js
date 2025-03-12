let modal = document.getElementsByClassName("modal_table")[0];
let modal_fields_new_table = document.getElementsByClassName("modal_add_field_new_table")[0];
let btn_add_table = document.getElementById("add_table");
let span_closed = document.getElementById("span_close");
let span_close_fields = document.getElementById("span_close_fields");
let input_add_table_name = document.getElementById("input_add_table");
let output_tablename_result = document.getElementById("name_resualt_output");
let accept_btn = document.getElementById("accept_btn");
let btn_add_new_field = document.getElementById("add_new_field");
let btn_send_data = document.getElementById("send_data");
let fields_container = document.getElementById("fields_container");

let table_name;
let json = {
    table_name: "",
    fields: {
        PartNumber: ""
    }
};

let open_modal = () => {
    modal.style.display = "block";
}

let close_modal = () => {
    modal.style.display = "none";
}

let open_modal_fields = () => {
    modal_fields_new_table.style.display = "block";
}

let close_modal_fields = () => {
    modal_fields_new_table.style.display = "none";
}

btn_add_table.onclick = open_modal;
span_closed.onclick = close_modal;
span_close_fields.onclick = close_modal_fields;

accept_btn.onclick = async function () {
    table_name = input_add_table_name.value;
    json.table_name = table_name;

    try {
        let response = await fetch("https://localhost:7205/Home/ValidationData", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(json)
        });

        if (!response.ok) {
            throw new Error(`Ошибка HTTP: ${response.status}`);
        }

        let result_data = await response.text();
        output_tablename_result.textContent = result_data;

        if (!document.querySelector('.next-step-btn')) {
            let next_btn = document.createElement('button');
            next_btn.setAttribute('type', 'button');
            next_btn.textContent = "Next step";
            next_btn.classList.add('next-step-btn');

            accept_btn.insertAdjacentElement('afterend', next_btn);

            next_btn.onclick = function () {
                close_modal();
                open_modal_fields();
            }
        }
    } catch (error) {
        console.error("Ошибка при выполнении запроса:", error);
        output_tablename_result.textContent = "Ошибка: " + error.message;
    }
};

// Логика для добавления новых полей
btn_add_new_field.onclick = function () {
    let newInput = document.createElement("input");
    newInput.type = "text";
    newInput.placeholder = "Новый столбец: ";
    newInput.classList.add("new-partnumber-input");
    fields_container.appendChild(newInput);
};

// Логика для отправки данных
btn_send_data.onclick = async function () {
    let inputs = document.querySelectorAll(".new-partnumber-input");
    inputs.forEach((input) => {
        let fieldName = input.value.trim();
        if (fieldName) {
            json.fields[fieldName] = "";
        }
    });

    try {
        let response = await fetch("https://localhost:7205/Home/AddTable", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(json)
        });

        if (!response.ok) {
            throw new Error(`Ошибка HTTP: ${response.status}`);
        }

        let result_data = await response.text();
        console.log("Ответ от сервера:", result_data);
        alert("Данные успешно отправлены!");
        close_modal_fields();
    } catch (error) {
        console.error("Ошибка при выполнении запроса:", error);
        alert("Ошибка: " + error.message);
    }
};