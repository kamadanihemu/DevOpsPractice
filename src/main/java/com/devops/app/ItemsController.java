package com.devops.app;

import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.web.bind.annotation.*;

import java.util.*;

@RestController
@RequestMapping("/api")
public class ItemsController {

  private final JdbcTemplate jdbc;

  public ItemsController(JdbcTemplate jdbc) {
    this.jdbc = jdbc;
    init();
  }

  private void init() {
    jdbc.execute("CREATE TABLE IF NOT EXISTS items (id SERIAL PRIMARY KEY, name TEXT NOT NULL)");
    Long count = jdbc.queryForObject("SELECT COUNT(*) FROM items", Long.class);
    if (count != null && count == 0) {
      jdbc.update("INSERT INTO items(name) VALUES (?)", "Hello from Java backend");
      jdbc.update("INSERT INTO items(name) VALUES (?)", "Postgres is connected");
    }
  }

  @GetMapping("/healthz")
  public Map<String, Object> healthz() {
    Long one = jdbc.queryForObject("SELECT 1", Long.class);
    return Map.of("status", "ok", "db", one);
  }

  @GetMapping("/items")
  public List<Map<String, Object>> items() {
    return jdbc.query("SELECT id, name FROM items ORDER BY id",
        (rs, rowNum) -> Map.of("id", rs.getInt("id"), "name", rs.getString("name")));
  }

  @PostMapping("/items")
  public Map<String, Object> add(@RequestBody Map<String, String> body) {
    String name = body.getOrDefault("name", "Unnamed");
    jdbc.update("INSERT INTO items(name) VALUES (?)", name);
    Integer id = jdbc.queryForObject("SELECT MAX(id) FROM items", Integer.class);
    return Map.of("id", id, "name", name);
  }
}
