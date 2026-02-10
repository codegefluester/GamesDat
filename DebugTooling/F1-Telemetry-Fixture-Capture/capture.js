import dgram from "dgram";
import { writeFileSync, mkdirSync } from "fs";

const socket = dgram.createSocket("udp4");
socket.bind(20777);

socket.on("message", (msg) => {
  // PacketHeader: packetFormat(2) + gameYear(1) + gameMajorVersion(1) + gameMinorVersion(1) + packetVersion(1) + packetId(1)
  const packetFormat = msg.readUInt16LE(0); // e.g., 2025, 2024
  const packetId = msg.readUInt8(6);

  const dir = `./fixtures/${packetFormat}`;
  mkdirSync(dir, { recursive: true });

  let filename = `packet-${packetId}.bin`;

  // Event packets (packetId 3) have a 4-character event code at offset 29 (after 29-byte header)
  if (packetId === 3) {
    const eventCode = msg.toString("ascii", 29, 33); // Read 4-byte event string (SSTA, FTLP, etc.)
    filename = `packet-3-${eventCode}.bin`;
  }

  writeFileSync(`${dir}/${filename}`, msg);
});
