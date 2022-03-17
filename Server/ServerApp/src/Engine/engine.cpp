#include "engine.h"

namespace engine
{
	void engine::_loop()
	{
		while (!_exit)
		{
			std::lock_guard<std::mutex> lg(_mut);

			for (auto& i : _battles)
			{
				auto post = i->_next();
				for (auto& j : post)
				{
					if (j.result != models::result::OKEY)
						_server->send_packet_async(j.conn, std::make_shared<packets::packet_error>(j.result));
					else
						_server->send_packet_async(j.conn, std::make_shared<packets::packet_json>(j.result, j.json_str));
				}
			}

			if (_time.count() + TIME_REMOVE_BATTLES < std::chrono::duration_cast<std::chrono::seconds>(std::chrono::system_clock::now().time_since_epoch()).count())
			{
				_remove_finished_buttles();
				_time = std::chrono::duration_cast<std::chrono::seconds>(std::chrono::system_clock::now().time_since_epoch());
			}
		}

		std::this_thread::sleep_for(std::chrono::milliseconds(1));
	}

	void engine::_remove_finished_buttles()
	{
		_battles.erase(
			std::remove_if(_battles.begin(), _battles.end(),
				[](const std::shared_ptr<battle>& o)
				{
					return o->get_game_state().finished;
				}),
			_battles.end());

		auto iter = _name_battle.begin();
		for (; iter != _name_battle.end(); )
		{
			if (iter->second.get()->get_game_state().finished)
				iter = _name_battle.erase(iter);
			else
				++iter;
		}
	}

	std::shared_ptr<battle> engine::_get_battle_by_conn(web::io_base::i_connection* conn)
	{
		auto iter_c = _con_name.find(conn);
		if (iter_c == _con_name.end())
			return nullptr;

		auto iter_n = _name_battle.find(iter_c->second);
		if (iter_n == _name_battle.end())
			return nullptr;

		return iter_n->second;
	}

	std::shared_ptr<battle> engine::_get_battle_by_name(const std::string& name)
	{
		if (name.empty()) return nullptr;
		for (auto& i : _battles)
			if (i->get_name() == name)
				return i;
		return nullptr;
	}

	engine::engine() : _time(std::chrono::seconds(0))
	{
		std::unique_ptr<thread::thread> _worker_thread(std::make_unique<thread::thread>());
		_thread.set_func(std::bind(&engine::_loop, this));
		_thread.set_exit([this]
			{
				_exit = true;
			}
		);
	}

	bool engine::run(web::io_server::i_server* server)
	{
		_server = server;
		_thread.run();
		return true;
	}

	int engine::get_total_games()
	{
		std::lock_guard<std::mutex> lg(_mut);
		return _battles.size();
	}

	std::tuple<models::result, models::player> engine::login(const models::login& login, web::io_base::i_connection* conn)
	{
		std::lock_guard<std::mutex> lg(_mut);

		auto iter_c = _con_name.find(conn);
		auto iter_n = _name_battle.find(login.name);

		if (iter_n != _name_battle.end())
		{
			auto battle = iter_n->second;
			auto player = battle->get_player_by_name(login.name);
			_con_name.insert(std::pair<web::io_base::i_connection*, std::string>(conn, login.name));
			if (!battle->update_player(player, conn))
				return { models::result::ACCESS_DENIED, {} };
			return { models::result::OKEY, player };
		}

		//create player
		models::player player;
		player.name = login.name;
		player.is_observer = login.is_observer;
		player.idx = _user_id++;

		auto ptr_battle = _get_battle_by_name(login.game);

		//create battle
		if (ptr_battle == nullptr)
		{
			ptr_battle = std::make_shared<battle>(login.game, login.num_players, login.num_turns);
			_battles.push_back(ptr_battle);
		}

		if (!ptr_battle->add_player(player, conn))
			return { models::result::ACCESS_DENIED, {} };

		_con_name.insert(std::pair<web::io_base::i_connection*, std::string>(conn, login.name));
		_name_battle.insert(std::pair<std::string, std::shared_ptr<battle>>(login.name, ptr_battle));

		return { models::result::OKEY, player };
	}

	models::result engine::logout(web::io_base::i_connection* conn)
	{
		std::lock_guard<std::mutex> lg(_mut);

		auto iter_c = _con_name.find(conn);
		if (iter_c == _con_name.end()) return models::result::BAD_COMMAND;
		auto iter_n = _name_battle.find(iter_c->second);

		if (iter_n != _name_battle.end())
			_name_battle.erase(iter_n);
		_con_name.erase(iter_c);
		return models::result::OKEY;
	}

	std::tuple<models::result, models::map> engine::map(web::io_base::i_connection* conn)
	{
		std::lock_guard<std::mutex> lg(_mut);

		auto battle = _get_battle_by_conn(conn);
		if (battle == nullptr) return { models::result::ACCESS_DENIED, {} };
		return { models::result::OKEY, battle->get_map() };
	}

	std::tuple<models::result, models::game_state> engine::game_state(web::io_base::i_connection* conn)
	{
		std::lock_guard<std::mutex> lg(_mut);

		auto battle = _get_battle_by_conn(conn);
		if (battle == nullptr) return { models::result::ACCESS_DENIED, {} };
		return { models::result::OKEY, battle->get_game_state() };
	}

	std::tuple<models::result, models::action_rsp> engine::actions(web::io_base::i_connection* conn)
	{
		std::lock_guard<std::mutex> lg(_mut);

		auto battle = _get_battle_by_conn(conn);
		if (battle == nullptr) return { models::result::ACCESS_DENIED, {} };
		return { models::result::OKEY, battle->get_actions() };
	}

	models::result engine::move(const models::move& move, web::io_base::i_connection* conn)
	{
		std::lock_guard<std::mutex> lg(_mut);

		auto battle = _get_battle_by_conn(conn);
		if (battle == nullptr) return models::result::BAD_COMMAND;
		return battle->set_move(move, conn);
	}

	models::result engine::shoot(const models::shoot& shoot, web::io_base::i_connection* conn)
	{
		std::lock_guard<std::mutex> lg(_mut);

		auto battle = _get_battle_by_conn(conn);
		if (battle == nullptr) return models::result::BAD_COMMAND;
		return battle->set_shoot(shoot, conn);
	}

	models::result engine::turn(web::io_base::i_connection* conn)
	{
		std::lock_guard<std::mutex> lg(_mut);

		auto battle = _get_battle_by_conn(conn);
		if (battle == nullptr) return models::result::BAD_COMMAND;
		return battle->set_turn(conn);
	}
}