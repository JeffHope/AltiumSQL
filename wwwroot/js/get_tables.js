//import { createAddComponentForm } from './add_comp_button.js';

var keys_json = new Set();
var headers_db_array = [];
var name_table;

async function getJson() {
    try {
        const response = await fetch("https://localhost:7205/Home/GetListTable");
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }

        const data = await response.json();
        const ltable = document.querySelector("#list_table");
        const tbody = document.createElement('tbody');
        let cnt = 0;

        data.forEach((item) => {
            const tr = document.createElement('tr');
            const td = document.createElement('td');
            const bt = document.createElement('button');
            bt.type = "button";
            bt.id = `btList${cnt}`;
            cnt++;
            bt.classList.add('btn');
            bt.textContent = item;
            td.appendChild(bt);
            tr.appendChild(td);
            tbody.appendChild(tr);
            ltable.appendChild(tbody);

            bt.addEventListener('click', async () => {
                name_table = item;
                await loadTableData(item);
            });
        });
    } catch (error) {
        console.error('Error fetching table list:', error);
    }
}

async function loadTableData(tableName) {
    try {
        const response = await fetch(`https://localhost:7205/Home/GetTable?name=${tableName}`);
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        let json =
        {

        }
        const dataJson = await response.json();
        const table = document.querySelector("#table_view");
        table.innerHTML = "";

        const thead = document.createElement('thead');
        dataJson.forEach((item) => {
            const tr = document.createElement('tr');
            for (const key in item) {
                if (!keys_json.has(key)) {
                    keys_json.add(key);
                    headers_db_array.push(key);
                    const td = document.createElement('td');
                    td.innerHTML = `<h5><b>${key}:</b></h5>&nbsp`;
                    tr.appendChild(td);
                }
            }
            thead.appendChild(tr);
        });
        table.appendChild(thead);
        const tbody = document.createElement('tbody');
        dataJson.forEach((item) => {
            const tr = document.createElement('tr');
            for (const value in item) {
                const td = document.createElement('td');
                td.innerHTML = `<h7>${item[value]}</h7>`;
                tr.appendChild(td);
                
            }
            tbody.appendChild(tr);
        });
        table.appendChild(tbody);
        dataJson.forEach((item) => {
            console.log(item);
        });
        keys_json.clear();
        import("./add_comp_button.js").then((module) => {
            module.createAddComponentForm(headers_db_array, tableName);
        });
        import("./del_comp.js")
            .then((promise) => {
                promise.del_comp(tableName, dataJson);
            });
        import("./edit_comp.js")
            .then((promise) => {
                promise.edit_comp(tableName, dataJson);
            });
        //import("./upgrade_edit_component.js")
        //    .then((promise) => {
        //        promise.edit_upgrade_comp(tableName, dataJson);
        //    });
    } catch (error) {
        console.error('Error loading table data:', error);
    }
}

getJson();
