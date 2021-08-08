// ==UserScript==
// @name         RaMSeS
// @namespace    http://tampermonkey.net/
// @version      1.1
// @description  Shows difficulty estimations of maps
// @author       Asuro
// @match        https://beatsaver.com/*
// @grant        GM_xmlhttpRequest
// @run-at       document-body
// @connect      splamy.de
// @connect      jsdelivr.net
// @require      https://cdn.jsdelivr.net/npm/chart.js@2.8.0
// @updateURL    https://github.com/Splamy/RateMapSeveritySaber/raw/master/Userscript/beatsaver.user.js
// @downloadURL  https://github.com/Splamy/RateMapSeveritySaber/raw/master/Userscript/beatsaver.user.js
// ==/UserScript==

//@ts-check
'use strict';

/**
 * @param {TrackInfo} trackInfo
 * @param {string} difficultyName 
 * @param {number[]} graphData
 */
function detailsModal(trackInfo, difficultyName, graphData) {
	let modalStyle = "background-color: #2F2F2F; border-radius: 1em; flex-direction: column;";
	document.body.insertAdjacentHTML("afterbegin", `
	<div class="modal ramses-graph ramses-close" style="display: block;">
		<div class="modal-dialog modal-dialog-centered rabbit-dialog" style="${modalStyle}">
			
			<div>${trackInfo.title}</div>
			<div style="height: 300px; width: 100%;">
				<canvas id="graph-canvas"></canvas>
			</div>
		</div>
	</div>
	`);
	document.querySelectorAll(".ramses-close").forEach(elem => elem.addEventListener("click", closeModal));
	let canvas = document.getElementById("graph-canvas");
	canvas.style.height = "7em";
	let ctx = canvas.getContext("2d");
	canvas.chart = new Chart(ctx, {
		type: "line",
		data: {
			labels: graphData.map(x => ""),
			datasets: [{
				label: difficultyName,
				backgroundColor: "rgb(255, 99, 132)",
				borderColor: "rgb(255, 99, 132)",
				data: graphData,
				fill: true,
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
	document.querySelectorAll(".ramses-graph").forEach(elem => elem.remove());
}

/** @typedef {{ id: string, title: string }} TrackInfo */
/** @typedef {{ maps: { difficulty: string, characteristic: string, maxDifficulty: number, avgDifficulty: number, graph: number[] }[] }} Score */

/**
 * @param {TrackInfo} trackInfo
 * @param {HTMLElement[]} diffs 
 * @param {Score} score
 */
function insertDifficulties(trackInfo, diffs, score, overview) {
	diffs.forEach(diff => {
		const img = diff.querySelector("img");
		const findCharacteristic = img.title;
		let findDiff = img.nextSibling.textContent;
		if (findDiff === "Expert+") {
			findDiff = "ExpertPlus";
		}

		const mapInfo = score.maps.find(mapInfo =>
			mapInfo.characteristic === findCharacteristic
			&& mapInfo.difficulty === findDiff);

		if (!mapInfo) {
			console.log("Didn't find mapinfo", diff, findCharacteristic, findDiff);
			return;
		}

		const avgDifficulty = formatNumber(mapInfo.avgDifficulty, 2);
		const maxDifficulty = formatNumber(mapInfo.maxDifficulty, 2);

		if (overview) {
			const tagHtml = `
				<div style="margin: 0 .5em; font-size: 1.3em;">☥</div>
				<div style="white-space: pre;">~${avgDifficulty}&nbsp;&nbsp;^${maxDifficulty}</div>
			`;
			diff.style.cursor = "pointer";
			diff.style.display = "inline-flex";
			diff.style.alignItems = "center";
			diff.insertAdjacentHTML("beforeend", tagHtml);
			diff.addEventListener("click", () => detailsModal(trackInfo, mapInfo.difficulty, mapInfo.graph));
		} else {
			const stats = diff.querySelector(".stats");
			const style = "font-size: 1.5em; font-weight: normal;";
			const tagHtml = `
				<span><b style="${style}">☥~</b> ${avgDifficulty}</span>
				<span><b style="${style}">☥^</b> ${maxDifficulty}</span>
			`;
			stats.insertAdjacentHTML("beforeend", tagHtml);
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

/** @type {Record<string,Score>} */
let cache = {};
let isUpdating = false;

async function getScore(mapId) {
	let json = cache[mapId];
	if (!json) {
		const response = await fetch2(`https://splamy.de/api/ramses/${mapId}`);
		json = JSON.parse(response);
		cache[mapId] = json;
	}
	return json;
}

async function scan() {
	if (isUpdating) return;
	isUpdating = true;
	let rescan = false;
	try {
		let searchResults = document.querySelector(".search-results");
		if (searchResults) {
			for (let map of searchResults.querySelectorAll(":scope > .beatmap:not(.ramses-matched)")) {
				map.classList.add("ramses-matched");

				const titleA = map.querySelector(".info > a");
				/** @type {TrackInfo} */
				const trackInfo = {
					id: titleA.href.split("/").pop(),
					title: titleA.innerText,
				}

				let json = await getScore(trackInfo.id);
				if (!map.isConnected) {
					rescan = true;
					break;
				}

				const diffsContainer = map.querySelector(".diffs");
				/** @type {HTMLElement[]} */
				const diffs = [...diffsContainer.querySelectorAll("span.badge")];
				insertDifficulties(trackInfo, diffs, json, true);
			}
		}

		let mapstats = document.querySelector(".mapstats");
		if (mapstats && !mapstats.classList.contains("ramses-matched")) {
			mapstats.classList.add("ramses-matched");

			/** @type {TrackInfo} */
			const trackInfo = {
				id: window.location.pathname.split("/").pop(),
				title: document.querySelector(".card-header").firstChild.textContent,
			}

			let json = await getScore(trackInfo.id);
			if (!mapstats.isConnected) {
				rescan = true;
				return;
			}

			/** @type {HTMLElement[]} */
			const diffs = [...mapstats.querySelectorAll(":scope > a")];
			insertDifficulties(trackInfo, diffs, json, false);
		}
	} finally {
		isUpdating = false;
	}

	if (rescan) {
		await scan();
	}
}

let observer = new MutationObserver((mutationList) => {
	scan();
});

observer.observe(document.querySelector("#root"), {
	attributes: false,
	childList: true,
	subtree: true,
});
