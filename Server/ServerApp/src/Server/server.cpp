#include "server.h"


namespace server
{
	void server::_cb_on_accepted(web::io_base::i_connection* conn, const SOCKET& socket)
	{
		total_connection++;
		printf("accepted: socket %d, total %d\n", socket, total_connection.load());
	}

	void server::_cb_on_recv(web::io_base::i_connection* conn, web::packet::packet_network* packet)
	{
		total_recved_b += packet->header.size + sizeof(packet->header);
		total_packet_r++;

		models::result result_code = models::result::BAD_COMMAND;
		std::string result_str = "";

		if (packet->header.type == models::action::LOGIN)
		{
			models::login data;
			std::string str_json((char*)packet->body, packet->header.size);
			nlohmann::json js_parser = nlohmann::json::parse(str_json);
			try
			{
				data = js_parser.get<models::login>();//maybe exception

				auto [res, player] = _engine.login(data, conn);
				result_code = res;

				if (result_code == models::result::OKEY)
				{
					const nlohmann::json j_player{ player };
					result_str = j_player[0].dump();
				}
			}
			catch (const std::exception& ex)
			{
				printf(ex.what());
			}
		}
		else if (packet->header.type == models::action::LOGOUT)
		{
			result_code = _engine.logout(conn);
		}
		else if (packet->header.type == models::action::MAP)
		{
			auto [res, map] = _engine.map(conn);
			result_code = res;
			if (result_code == models::result::OKEY)
			{
				nlohmann::json j_map{ map };
				result_str = j_map[0].dump();
			}
		}
		else if (packet->header.type == models::action::GAME_STATE)
		{
			auto [res, game_state] = _engine.game_state(conn);
			result_code = res;
			if (result_code == models::result::OKEY)
			{
				nlohmann::json j_map{ game_state };
				result_str = j_map[0].dump();
			}
		}
		else if (packet->header.type == models::action::GAME_ACTIONS)
		{
			auto [res, actions] = _engine.actions(conn);
			result_code = res;
			if (result_code == models::result::OKEY)
			{
				nlohmann::json j_map{ actions };
				result_str = j_map[0].dump();
			}
		}
		else if (packet->header.type == models::action::MOVE)
		{
			models::move data;
			std::string str_json((char*)packet->body, packet->header.size);
			nlohmann::json js_parser = nlohmann::json::parse(str_json);
			try
			{
				data = js_parser.get<models::move>();//maybe exception
				result_code = _engine.move(data, conn);
			}
			catch (const std::exception& ex)
			{
				printf(ex.what());
			}
		}
		else if (packet->header.type == models::action::SHOOT)
		{
			models::shoot data;
			std::string str_json((char*)packet->body, packet->header.size);
			nlohmann::json js_parser = nlohmann::json::parse(str_json);
			try
			{
				data = js_parser.get<models::shoot>();//maybe exception
				result_code = _engine.shoot(data, conn);
			}
			catch (const std::exception& ex)
			{
				printf(ex.what());
			}
		}
		else if (packet->header.type == models::action::TURN)
		{
			result_code = _engine.turn(conn);
			if (result_code == models::result::OKEY)
				return;
		}

		if (result_code != models::result::OKEY)
			_server.send_packet_async(conn, std::make_shared<packets::packet_error>(result_code));
		else
			_server.send_packet_async(conn, std::make_shared<packets::packet_json>(result_code, result_str));

	}

	void server::_cb_on_send(web::io_base::i_connection* conn, web::packet::packet_network* packet)
	{
		total_sended_b += packet->header.size + sizeof(packet->header);
		total_packet_s++;
	}

	void server::_cb_on_disconnected(web::io_base::i_connection* conn)
	{
		total_connection--;
		printf("disconnected: socket %d, total %d\n", conn->get_socket(), total_connection.load());
	}

	server::server(web::io_server::i_server& server) : _server(server)
	{
		_server.set_on_accepted(std::bind(&server::_cb_on_accepted, this, std::placeholders::_1, std::placeholders::_2));
		_server.set_on_recv(std::bind(&server::_cb_on_recv, this, std::placeholders::_1, std::placeholders::_2));
		_server.set_on_send(std::bind(&server::_cb_on_send, this, std::placeholders::_1, std::placeholders::_2));
		_server.set_on_disconnected(std::bind(&server::_cb_on_disconnected, this, std::placeholders::_1));
	}

	bool server::run()
	{
		if (_server.run() && _engine.run(&_server))
			return true;
		return false;
	}
	void server::status()
	{
		printf("connection: %d\ngames: %d\npackts (r/s): %d / %d\ndata (r/s): %d / %d\n",
			total_connection.load(),_engine.get_total_games(), (int)total_packet_r.load(), (int)total_packet_s.load(), (int)total_recved_b.load(), (int)total_sended_b.load());
	}
}