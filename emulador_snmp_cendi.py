import socket
import threading
import time

# Agentes SNMPv2c de las 13 impresoras reales de RG StarGas Cendi
IMPRESORAS = [
    {"id": 1,  "ip": "127.0.0.11", "puerto": 16101, "modelo": "MF264 Stargas",              "toner": 28, "contador": 146521, "online": True},
    {"id": 2,  "ip": "127.0.0.12", "puerto": 16102, "modelo": "MF455 Overland",             "toner": 55, "contador": 2091,   "online": True},
    {"id": 3,  "ip": "127.0.0.13", "puerto": 16103, "modelo": "MF455 Traco",                "toner": 12, "contador": 84225,  "online": True},
    {"id": 4,  "ip": "127.0.0.14", "puerto": 16104, "modelo": "MF264 Rrhh",                 "toner": 64, "contador": 30047,  "online": True},
    {"id": 5,  "ip": "127.0.0.15", "puerto": 16105, "modelo": "RICOH MP3350 Cendi",         "toner": 82, "contador": 591523, "online": True},
    {"id": 6,  "ip": "127.0.0.16", "puerto": 16106, "modelo": "Facturacion IR1643 Total",   "toner": 9,  "contador": 261888, "online": True},
    {"id": 7,  "ip": "127.0.0.17", "puerto": 16107, "modelo": "Facturacion IR1643 Stargas", "toner": 14, "contador": 247246, "online": True},
    {"id": 8,  "ip": "127.0.0.18", "puerto": 16108, "modelo": "MF264 PCP",                  "toner": 45, "contador": 192748, "online": True},
    {"id": 9,  "ip": "127.0.0.19", "puerto": 16109, "modelo": "MF264 Overland",             "toner": 71, "contador": 68509,  "online": True},
    {"id": 10, "ip": "127.0.0.20", "puerto": 16110, "modelo": "MF455 Principal CENDI",      "toner": 38, "contador": 57551,  "online": True},
    {"id": 11, "ip": "127.0.0.21", "puerto": 16111, "modelo": "MF264 Comercial OV-JD",      "toner": 76, "contador": 34151,  "online": True},
    {"id": 12, "ip": "127.0.0.24", "puerto": 16112, "modelo": "IR1643 RRHH",                "toner": 22, "contador": 17897,  "online": True},
    {"id": 13, "ip": "127.0.0.23", "puerto": 16113, "modelo": "MF264 Comercial",            "toner": 89, "contador": 34573,  "online": True},
]

mostrar_tramas = False

def encode_length(length):
    if length < 0x80:
        return bytes([length])
    s = []
    while length > 0:
        s.insert(0, length & 0xFF)
        length >>= 8
    return bytes([0x80 | len(s)] + s)

def decode_length(data, idx):
    b = data[idx]
    idx += 1
    if (b & 0x80) == 0:
        return b, idx
    num_bytes = b & 0x7F
    length = 0
    for _ in range(num_bytes):
        length = (length << 8) | data[idx]
        idx += 1
    return length, idx

def encode_tlv(tag, value_bytes):
    return bytes([tag]) + encode_length(len(value_bytes)) + value_bytes

def decode_tlv(data, idx):
    tag = data[idx]
    length, idx = decode_length(data, idx + 1)
    val = data[idx:idx + length]
    return tag, val, idx + length

def encode_integer(val):
    val = max(0, int(val))
    if val == 0:
        return encode_tlv(0x02, b'\x00')
    b = []
    v = val
    while v > 0:
        b.insert(0, v & 0xFF)
        v >>= 8
    if b[0] & 0x80:
        b.insert(0, 0x00)
    return encode_tlv(0x02, bytes(b))

def encode_octet_string(s):
    return encode_tlv(0x04, str(s).encode('utf-8'))

def encode_oid(oid_str):
    parts = [int(x) for x in oid_str.strip('.').split('.')]
    out = [parts[0] * 40 + parts[1]]
    for p in parts[2:]:
        if p == 0:
            out.append(0)
        else:
            sub = []
            while p > 0:
                sub.insert(0, p & 0x7F)
                p >>= 7
            for i in range(len(sub) - 1):
                sub[i] |= 0x80
            out.extend(sub)
    return encode_tlv(0x06, bytes(out))

def decode_oid(val_bytes):
    if not val_bytes:
        return ""
    first = val_bytes[0]
    parts = [first // 40, first % 40]
    val = 0
    for b in val_bytes[1:]:
        val = (val << 7) | (b & 0x7F)
        if not (b & 0x80):
            parts.append(val)
            val = 0
    return ".".join(str(x) for x in parts)

def construir_mib(imp):
    return {
        "1.3.6.1.2.1.1.1.0":            ("str", imp["modelo"]),
        "1.3.6.1.2.1.43.10.2.1.4.1.1":  ("int", imp["contador"]),
        "1.3.6.1.2.1.43.11.1.1.6.1.1":  ("str", "Black Toner Cartridge"),
        "1.3.6.1.2.1.43.11.1.1.8.1.1":  ("int", 100),
        "1.3.6.1.2.1.43.11.1.1.9.1.1":  ("int", imp["toner"]),
    }

def procesar_paquete_snmp(data, imp):
    tag, seq_val, _ = decode_tlv(data, 0)
    if tag != 0x30:
        return None
    idx = 0
    _, ver_bytes, idx = decode_tlv(seq_val, idx)
    _, comm_bytes, idx = decode_tlv(seq_val, idx)
    pdu_tag, pdu_val, idx = decode_tlv(seq_val, idx)
    if pdu_tag not in (0xA0, 0xA1):
        return None

    p_idx = 0
    req_id_tag, req_id_bytes, p_idx = decode_tlv(pdu_val, p_idx)
    _, _, p_idx = decode_tlv(pdu_val, p_idx)
    _, _, p_idx = decode_tlv(pdu_val, p_idx)
    _, vb_list_bytes, p_idx = decode_tlv(pdu_val, p_idx)

    mib = construir_mib(imp)
    sorted_oids = sorted(mib.keys(), key=lambda x: [int(n) for n in x.split('.')])

    v_idx = 0
    resp_varbinds = b''
    while v_idx < len(vb_list_bytes):
        _, vb_bytes, v_idx = decode_tlv(vb_list_bytes, v_idx)
        _, oid_bytes, _ = decode_tlv(vb_bytes, 0)
        req_oid = decode_oid(oid_bytes)

        target_oid = req_oid
        if pdu_tag == 0xA1:
            req_parts = [int(n) for n in req_oid.split('.')]
            next_oid = None
            for cand in sorted_oids:
                if [int(n) for n in cand.split('.')] > req_parts:
                    next_oid = cand
                    break
            target_oid = next_oid

        if target_oid and target_oid in mib:
            val_type, val_data = mib[target_oid]
            val_encoded = encode_octet_string(val_data) if val_type == 'str' else encode_integer(val_data)
            vb_resp = encode_oid(target_oid) + val_encoded
        else:
            end_tag = 0x82 if pdu_tag == 0xA1 else 0x80
            vb_resp = encode_oid(req_oid) + bytes([end_tag, 0x00])

        resp_varbinds += encode_tlv(0x30, vb_resp)

        if mostrar_tramas:
            tipo_pdu = "GET-NEXT" if pdu_tag == 0xA1 else "GET"
            print(f"  [SNMPv2c {tipo_pdu}] {imp['ip']} ({imp['modelo']}) -> OID {req_oid}")

    pdu_body = (
        encode_tlv(req_id_tag, req_id_bytes) +
        encode_integer(0) +
        encode_integer(0) +
        encode_tlv(0x30, resp_varbinds)
    )
    resp_pdu = encode_tlv(0xA2, pdu_body)
    resp_msg = encode_tlv(0x02, ver_bytes) + encode_tlv(0x04, comm_bytes) + resp_pdu
    return encode_tlv(0x30, resp_msg)

def escuchar_socket(sock, imp):
    while True:
        try:
            data, addr = sock.recvfrom(4096)
            if not imp["online"]:
                continue
            respuesta = procesar_paquete_snmp(data, imp)
            if respuesta:
                sock.sendto(respuesta, addr)
        except Exception:
            time.sleep(0.1)

def iniciar_agentes():
    for imp in IMPRESORAS:
        # 1. Puerto puente UDP (compatible con Docker y ejecución local sin requerir permisos de Administrador)
        try:
            s_bridge = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            s_bridge.bind(("0.0.0.0", imp["puerto"]))
            threading.Thread(target=escuchar_socket, args=(s_bridge, imp), daemon=True).start()
        except Exception as e:
            print(f"Aviso en puerto {imp['puerto']}: {e}")

        # 2. Puerto estándar SNMP 161 en IP loopback dedicada (si el puerto 161 está libre en Windows)
        try:
            s_std = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            s_std.bind((imp["ip"], 161))
            threading.Thread(target=escuchar_socket, args=(s_std, imp), daemon=True).start()
        except Exception:
            pass

def imprimir_tabla():
    print("\n" + "=" * 88)
    print(f"{'ID':<4} {'IP RED':<13} {'MODELO / IMPRESORA':<28} {'ESTADO':<13} {'TÓNER':<8} {'CONTADOR PÁGS'}")
    print("-" * 88)
    for imp in IMPRESORAS:
        estado = "EN LÍNEA" if imp["online"] else "APAGADA"
        print(f"{imp['id']:<4} {imp['ip']:<13} {imp['modelo']:<28} {estado:<13} {str(imp['toner'])+'%':<8} {imp['contador']:,}")
    print("=" * 88)

def menu():
    global mostrar_tramas
    print("\n🚀 EMULADOR EXTERNO DE AGENTES SNMPv2c (RFC 3805 - Printer MIB) INICIADO")
    imprimir_tabla()
    print("\nComandos en vivo para tu demostración ante el jurado:")
    print("  estado                  -> Ver tabla actual de las 13 impresoras")
    print("  toner <ID> <0-100>      -> Cambiar % de tóner en vivo (ej: toner 1 5)")
    print("  imprimir <ID> <paginas> -> Simular impresión de páginas (ej: imprimir 3 4000)")
    print("  apagar <ID>             -> Desconectar una impresora de la red (ej: apagar 1)")
    print("  encender <ID>           -> Reconectar una impresora a la red (ej: encender 1)")
    print("  apagar todas            -> Apagar todas las impresoras")
    print("  encender todas          -> Encender todas las impresoras")
    print("  tramas                  -> Activar/desactivar vista en vivo de paquetes SNMP")
    print("  salir                   -> Cerrar el simulador\n")

    while True:
        try:
            cmd = input("SNMP-CENDI> ").strip()
        except (EOFError, KeyboardInterrupt):
            break

        if not cmd:
            continue

        partes = cmd.split()
        accion = partes[0].lower()

        if accion == "salir":
            break
        elif accion == "estado":
            imprimir_tabla()
        elif accion == "tramas":
            mostrar_tramas = not mostrar_tramas
            print(f"Vista de paquetes SNMP en vivo: {'ACTIVADA' if mostrar_tramas else 'DESACTIVADA'}")
        elif accion in ("apagar", "encender") and len(partes) >= 2:
            nuevo_estado = (accion == "encender")
            if partes[1].lower() == "todas":
                for imp in IMPRESORAS:
                    imp["online"] = nuevo_estado
                imprimir_tabla()
            elif partes[1].isdigit():
                idx = int(partes[1])
                imp = next((x for x in IMPRESORAS if x["id"] == idx), None)
                if imp:
                    imp["online"] = nuevo_estado
                    print(f"-> {imp['modelo']} ({imp['ip']}) ahora está {'EN LÍNEA' if nuevo_estado else 'APAGADA'}.")
        elif accion == "toner" and len(partes) >= 3 and partes[1].isdigit() and partes[2].isdigit():
            idx = int(partes[1])
            nivel = max(0, min(100, int(partes[2])))
            imp = next((x for x in IMPRESORAS if x["id"] == idx), None)
            if imp:
                imp["toner"] = nivel
                print(f"-> OID 1.3.6.1.2.1.43.11.1.1.9.1.1 ({imp['modelo']}) actualizado a {nivel}%.")
        elif accion == "imprimir" and len(partes) >= 3 and partes[1].isdigit() and partes[2].isdigit():
            idx = int(partes[1])
            pags = max(1, int(partes[2]))
            imp = next((x for x in IMPRESORAS if x["id"] == idx), None)
            if imp:
                imp["contador"] += pags
                baja_toner = max(0, round((pags / 3500.0) * 100))
                imp["toner"] = max(1, imp["toner"] - baja_toner)
                print(f"-> {imp['modelo']} imprimió +{pags:,} págs. Nuevo contador OID: {imp['contador']:,} | Tóner: {imp['toner']}%.")
        else:
            print("Comando no reconocido. Usa: estado | toner <ID> <%> | imprimir <ID> <pags> | apagar <ID> | encender <ID> | tramas")

if __name__ == "__main__":
    iniciar_agentes()
    menu()