
var LibraryWebSocket = {
	$webSocketState: {
		/*
		 * instance of ws
		 *
		 * Instance structure:
		 * {
		 * 	url: string,
		 * 	ws: WebSocket
		 * }
		 */
		instance: {},
		defaultUrl: "ws://localhost:42025",

		/* Event listeners */
		onOpen: null,
		onMesssage: null,
		onError: null,
		onClose: null,

		/* Debug mode */
		debug: false
	},

	/**
	 * Set onOpen callback
	 *
	 * @param callback Reference to C# static function
	 */
	wsSetOnOpen: function(callback) {
		webSocketState.onOpen = callback;
	},

	/**
	 * Set onMessage callback
	 *
	 * @param callback Reference to C# static function
	 */
	wsSetOnMessage: function(callback) {
		webSocketState.onMessage = callback;
	},

	/**
	 * Set onError callback
	 *
	 * @param callback Reference to C# static function
	 */
	wsSetOnError: function(callback) {
		webSocketState.onError = callback;
	},

	/**
	 * Set onClose callback
	 *
	 * @param callback Reference to C# static function
	 */
	wsSetOnClose: function(callback) {
		webSocketState.onClose = callback;
	},

	/**
	 * Connect WebSocket to the server
	 *
	 * @param host host address
	 */
	wsConnect: function(host) {

		var instance = webSocketState.instance;
		if (!instance) return -1;

		if (instance.ws)
			return -2;

		instance.url = host != null ? `ws://${UTF8ToString(host)}:42025` : webSocketState.defaultUrl;
		instance.ws = new WebSocket(instance.url);

		instance.ws.binaryType = 'arraybuffer';

		instance.ws.onopen = function() {

			if (webSocketState.debug)
				console.log("[JSLIB WebSocket] Connected.");

			if (webSocketState.onOpen)
				Module.dynCall_v(webSocketState.onOpen);

		};

		instance.ws.onmessage = function(ev) {

			if (webSocketState.debug)
				console.log("[JSLIB WebSocket] Received message:", ev.data);

			if (webSocketState.onMessage === null)
				return;

			if (ev.data instanceof ArrayBuffer) {

				var dataBuffer = new Uint8Array(ev.data);

				var buffer = _malloc(dataBuffer.length);
				HEAPU8.set(dataBuffer, buffer);

				try {
					Module.dynCall_vii(webSocketState.onMessage, buffer, dataBuffer.length);
				} finally {
					_free(buffer);
				}

      		} else {
				var dataBuffer = (new TextEncoder()).encode(ev.data);

				var buffer = _malloc(dataBuffer.length);
				HEAPU8.set(dataBuffer, buffer);

				try {
					Module.dynCall_vii(webSocketState.onMessage, buffer, dataBuffer.length);
				} finally {
					_free(buffer);
				}
      		}

		};

		instance.ws.onerror = function(ev) {

			if (webSocketState.debug)
				console.log("[JSLIB WebSocket] Error occured.");

			if (webSocketState.onError) {

				var msg = "WebSocket error.";
				var length = lengthBytesUTF8(msg) + 1;
				var buffer = _malloc(length);
				stringToUTF8(msg, buffer, length);

				try {
					Module.dynCall_vi(webSocketState.onError, buffer);
				} finally {
					_free(buffer);
				}
			}

		};

		instance.ws.onclose = function(ev) {

			if (webSocketState.debug)
				console.log("[JSLIB WebSocket] Closed.");

			if (webSocketState.onClose)
				Module.dynCall_vi(webSocketState.onClose, ev.code);

			delete instance.ws;
		};

		return 0;

	},

	/**
	 * Close WebSocket connection
	 */
	wsClose: function() {

		var instance = webSocketState.instance;
		if (!instance) return -1;

		if (!instance.ws)
			return -3;

		if (instance.ws.readyState === 2)
			return -4;

		if (instance.ws.readyState === 3)
			return -5;

		try {
			instance.ws.close(1000);
		} catch(err) {
			return -7;
		}

		return 0;

	},

	/**
	 * Send message over WebSocket
	 *
	 * @param bufferPtr Pointer to the message buffer
	 * @param length Length of the message in the buffer
	 */
	wsSend: function(bufferPtr, length) {

		var instance = webSocketState.instance;
		if (!instance) return -1;

		if (!instance.ws)
			return -3;

		if (instance.ws.readyState !== 1)
			return -6;

		instance.ws.send(HEAPU8.buffer.slice(bufferPtr, bufferPtr + length));

		return 0;

	},

	/**
	 * Send text message over WebSocket
	 *
	 * @param message Text message
	 */
	wsSendText: function(message) {

		var instance = webSocketState.instance;
		if (!instance) return -1;

		if (!instance.ws)
			return -3;

		if (instance.ws.readyState !== 1)
			return -6;

		instance.ws.send(UTF8ToString(message));
		return 0;
	},

	/**
	 * Return WebSocket readyState
	 *
	 */
	wsGetState: function() {

		var instance = webSocketState.instance;
		if (!instance) return -1;

		if (instance.ws)
			return instance.ws.readyState;
		else
			return 3;
	}

};

autoAddDeps(LibraryWebSocket, '$webSocketState');
mergeInto(LibraryManager.library, LibraryWebSocket);
