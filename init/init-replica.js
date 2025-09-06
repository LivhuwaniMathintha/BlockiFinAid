try {
  rs.initiate({
    _id: "rs0",
    members: [{ _id: 0, host: "mongo:27017" }]
  });
} catch (e) {
  rs.status();
}