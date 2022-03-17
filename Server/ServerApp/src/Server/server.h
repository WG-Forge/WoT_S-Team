#pragma once

#include "../Engine/engine.h"


#include <string>
#include <vector>
#include <mutex>
#include <atomic>

namespace server
{
	class server
	{
	public:
		server(web::io_server::i_server& server);
		bool run();
		void status();
	private:
		web::io_server::i_server& _server;
		engine::engine _engine;

		std::recursive_mutex mut_conn;
		std::vector<web::io_base::i_connection*> _connestions;

		std::atomic<unsigned long long> total_packet_r = 0;
		std::atomic<unsigned long long> total_packet_s = 0;
		std::atomic<unsigned long long> total_recved_b = 0;
		std::atomic<unsigned long long> total_sended_b = 0;
		std::atomic<int> total_connection = 0;

		void _cb_on_accepted(web::io_base::i_connection* conn, const SOCKET& socket);
		void _cb_on_recv(web::io_base::i_connection* conn, web::packet::packet_network* packet);
		void _cb_on_send(web::io_base::i_connection* conn, web::packet::packet_network* packet);
		void _cb_on_disconnected(web::io_base::i_connection* conn);
	};
}
