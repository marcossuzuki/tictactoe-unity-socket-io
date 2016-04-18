var port = 1337;
var server = require('http').createServer();
var io = require('socket.io')(server);
var users = [];

server.listen(port, function (req, res) {
	console.log("Server running at http://127.0.0.1:" + port);
});

io.on('connection', function (socket) {
	console.log('a user has connected');

	users.push(socket);
	var id = users.length - 1;
	console.log(id);

	if (id % 2 != 0) {
		users[id].opponent = users[id - 1];
		users[id - 1].opponent = users[id];
		users[id].emit('startGame', { color: 'red', id: id, oppId: id - 1 })
		users[id].opponent.emit('startGame', { color: 'blue', id: id - 1, oppId: id })
	}
	
	socket.on('move', function (data) {		
		socket.opponent.emit('move', { color: data.color, movePos: data.movePos });
	});
	
	socket.on('endgame', function (data) {
		console.log(data);
		socket.opponent.emit('endgame', { winner: data.winner });
	});
	
	socket.on('restart', function (data) {
		socket.opponent.emit('restart', { id: '' });
	});
	
	socket.on('disconnect', function () {
		console.log('user disconnected');
	});
});


