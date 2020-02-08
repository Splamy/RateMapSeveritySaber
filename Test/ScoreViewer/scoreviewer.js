function time_to_date(time) {
	// "00:01:03.4824218"
	return moment("0001-01-01 " + time, "YYYY-MM-DD HH:mm:ss:S");
}

function kind_to_data(song, color, kind, display_color, add) {
	const data = [];
	for (const bloq of (color === "red" ? song.DataRed : song.DataBlue)) {
		data.push({
			x: time_to_date(bloq.Time),
			y: Number(bloq[kind])
		});
	}

	return Object.assign({
		label: `${color} ${kind}`,
		fill: false,
		borderColor: display_color,
		data: data,
		showLine: true,
		pointRadius: 0,
		type: 'line',
	}, add);
}

function map_to_data(song) {
	const hit_add = { showLine: true, pointRadius: 0, borderWidth: 1 };
	const cont_add = { showLine: true, pointRadius: 0, borderWidth: 1 };
	return [
		kind_to_data(song, "red", "HitDifficulty", "red", hit_add),
		kind_to_data(song, "red", "ContinuousDifficulty", "darkred", cont_add),
		kind_to_data(song, "blue", "HitDifficulty", "blue", hit_add),
		kind_to_data(song, "blue", "ContinuousDifficulty", "darkblue", cont_add),
	];
}

function do_graph() {
	const main = document.getElementById("main");
	if (typeof scores === "undefined") {
		main.innerText = "Could not find data set. Make sure it's generated, dummy :P";
		return;
	}

	for (const song of scores) {
		//const name = song.Name;

		const chart_canvas = document.createElement("canvas");
		chart_canvas.height = 100;
		const ctx = chart_canvas.getContext('2d');

		const graph_data_collection = map_to_data(song);

		const chart = new Chart(ctx, {
			// The type of chart we want to create
			type: 'line',

			// The data for our dataset
			data: {
				datasets: graph_data_collection
			},
			options: {
				title: {
					display: true,
					text: `${song.Name} - ${song.DifficultyName}`
				},
				scales: {
					xAxes: [{
						type: 'time',
						time: {
							unit: 'second'
						}
					}]
				},
				elements: {
					line: {
						tension: 0
					}
				}
			}
		});

		main.appendChild(chart_canvas);
	}
}

window.onload = do_graph;
