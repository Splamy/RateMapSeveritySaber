// ==UserScript==
// @name         Cooles Beatsaver Script
// @namespace    http://tampermonkey.net/
// @version      0.1
// @description  try to take over the world!
// @author       Asuro
// @match        https://beatsaver.com/browse/*
// @grant        GM_xmlhttpRequest
// @run-at       document-body
// @connect      80d18138.ngrok.io
// ==/UserScript==

/**
 * @param {HTMLElement} beatmapContentElement 
 * @param {Array<any>} difficultyMap 
 */
function insertDifficulties(beatmapContentElement, maps) {
    maps.forEach(mapInfo => {
        let difficulty = mapInfo.difficulty.toLowerCase();
        if (difficulty === "expertplus") {
            difficulty = "expert-plus";
        }
        let targetTag = beatmapContentElement.querySelector(`.is-${difficulty}`);
        let avgDifficulty = formatNumber(mapInfo.avgDifficulty, 2);
        let maxDifficulty = formatNumber(mapInfo.maxDifficulty, 2);

        if (targetTag) {
            let ankhStyle = "margin-right: .5em; font-size: 1.5em";
            let tagHtml = `
                <span class="tag is-${difficulty}" title="~Average Difficulty, ^Max Difficulty">
                    <b style="${ankhStyle}">☥</b>
                    <span style="margin-right: .8em">~${avgDifficulty}</span>
                <span>^${maxDifficulty}</span>
            `;
            targetTag.insertAdjacentHTML("afterend", tagHtml);
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
                        if (element.difficultyCache) {
                            insertDifficulties(element, element.difficultyCache);
                        } else {
                            let id = element.querySelector(".stats li").innerText.split(" ")[0];
                            fetch2(`https://80d18138.ngrok.io/api/ramses/${id}`).then(response => {
                                let json = JSON.parse(response);
                                element.difficultyCache = json.maps;
                                insertDifficulties(element, element.difficultyCache);
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