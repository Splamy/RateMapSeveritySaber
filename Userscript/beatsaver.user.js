// ==UserScript==
// @name         Cooles Beatsaver Script
// @namespace    http://tampermonkey.net/
// @version      0.1
// @description  Shows difficulty estimations of maps
// @author       Asuro
// @match        https://beatsaver.com/*
// @grant        GM_xmlhttpRequest
// @run-at       document-body
// @connect      splamy.de
// @connect      jsdelivr.net
// @require      https://cdn.jsdelivr.net/npm/chart.js@2.8.0
// ==/UserScript==

function detailsModal(graphData) {
    let modalStyle = "width: 90%";
    document.body.insertAdjacentHTML("afterbegin", `
    <div class="modal is-active">
        <div class="modal-background"></div>
        <div class="modal-content" style="${modalStyle}">
            <div class="box">
                <canvas id="graph-canvas"></canvas>
            </div>
        </div>
        <button class="modal-close is-large" aria-label="close"></button>
    </div>
    `);
    document.querySelectorAll(".modal-background").forEach(elem => elem.addEventListener("click", closeModal));
    document.querySelectorAll(".modal-close").forEach(elem => elem.addEventListener("click", closeModal));
    let canvas = document.getElementById("graph-canvas");
    canvas.style.height = "10vh";
    let ctx = canvas.getContext("2d");
    canvas.chart = new Chart(ctx, {
        type: "line",
        data: {
            labels: graphData.map(x => ""),
            datasets: [{
                label: "Difficulty",
                backgroundColor: "rgb(255, 99, 132)",
                borderColor: "rgb(255, 99, 132)",
                data: graphData,
                fill: false,
                pointRadius: 0,
            }]
        },
        options: {
            maintainAspectRatio: false,
            scales: {
                xAxes: [{
                    gridLines: {
                        display: false,
                    }
                }]
            }
        }
    });
}

function closeModal() {
    document.querySelectorAll(".modal").forEach(elem => elem.remove());
}

/**
 * @param {String} id
 * @param {HTMLElement} beatmapContentElement 
 * @param {Array<any>} maps 
 */
function insertDifficulties(id, beatmapContentElement, maps) {
    maps.forEach(mapInfo => {
        let difficulty = mapInfo.difficulty.toLowerCase();
        if (difficulty === "expertplus") {
            difficulty = "expert-plus";
        }
        let targetTag = beatmapContentElement.querySelector(`.is-${difficulty}`);
        let avgDifficulty = formatNumber(mapInfo.avgDifficulty, 2);
        let maxDifficulty = formatNumber(mapInfo.maxDifficulty, 2);

        if (targetTag) {
            let tagStyle = "cursor: hand";
            let ankhStyle = "margin-right: .5em; font-size: 1.5em";
            let htmlId = `${difficulty}${id}`
            let tagHtml = `
                <span id="${htmlId}" class="tag is-${difficulty}" title="~Average Difficulty, ^Max Difficulty" style="${tagStyle}">
                    <b style="${ankhStyle}">☥</b>
                    <span style="margin-right: .8em">~${avgDifficulty}</span>
                <span>^${maxDifficulty}</span>
            `;
            targetTag.insertAdjacentHTML("afterend", tagHtml);
            document.querySelector("#" + htmlId).addEventListener("click", () => detailsModal(mapInfo.graph));
        } else {
            console.warn(`Didn't find tag!`);
        }
    });
}

function formatNumber(num, digits) {
    if (digits === undefined) digits = 2;
    return num.toLocaleString("en", { minimumFractionDigits: digits, maximumFractionDigits: digits });
}

/**
 * @param {string} url
 * @returns {string | undefined}
 */
function getHostName(url) {
    var match = url.match(/:\/\/([^/:]+)/i);
    if (match && match.length > 1 && typeof match[1] === 'string' && match[1].length > 0) {
        return match[1];
    }
    else {
        return undefined;
    }
}

/**
 * @param {string} url
 * @returns {Promise<string>}
 */
function fetch2(url) {
    return new Promise(function (resolve, reject) {
        let host = getHostName(url);
        let request_param = {
            method: "GET",
            url: url,
            headers: { "Origin": host },
            onload: (req) => {
                if (req.status >= 200 && req.status < 300) {
                    resolve(req.responseText);
                } else {
                    reject();
                }
            },
            onerror: () => {
                reject();
            }
        };
        GM_xmlhttpRequest(request_param);
    });
}

(function () {
    'use strict';

    let observer = new MutationObserver((mutationList) => {
        mutationList.forEach(mut => {
            if (mut.target.nodeType === mut.target.ELEMENT_NODE) {
                /** @type {HTMLElement} */
                let element = mut.target;

                if (element.classList.contains("beatmap-result")) {
                    if (mut.previousSibling === null) {
                        let id = element.querySelector(".stats li").innerText.split(" ")[0];

                        if (element.difficultyCache) {
                            insertDifficulties(id, element, element.difficultyCache);
                        } else {
                            fetch2(`https://splamy.de/api/ramses/${id}`).then(response => {
                                let json = JSON.parse(response);
                                element.difficultyCache = json.maps;
                                insertDifficulties(id, element, element.difficultyCache);
                            });
                        }
                    }
                }
            }
        });
    });
    observer.observe(document.querySelector("body"), {
        attributes: false,
        childList: true,
        subtree: true,
    });
})();