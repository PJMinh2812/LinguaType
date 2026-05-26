import React, { useEffect, useState } from "react";
import axios from "axios";

export default function App() {
  const [samples, setSamples] = useState([]);
  const [showPinyin, setShowPinyin] = useState(true);
  const [customText, setCustomText] = useState("");
  // translation feature removed
  const [customSample, setCustomSample] = useState(null);

  useEffect(() => {
    axios.get("/api/samples").then((r) => setSamples(r.data));
  }, []);

  return (
    <div style={{ padding: 20, fontFamily: "sans-serif" }}>
      <h1>LinguaType — Practice Typing Chinese</h1>
      <label>
        <input
          type="checkbox"
          checked={showPinyin}
          onChange={(e) => setShowPinyin(e.target.checked)}
        />{" "}
        Show pinyin
      </label>
      <div style={{ marginTop: 20 }}>
        <div
          style={{ marginBottom: 18, padding: 12, border: "1px solid #eee" }}
        >
          <div style={{ fontWeight: 600, marginBottom: 6 }}>
            Practice with your own text
          </div>
          <textarea
            rows={3}
            style={{ width: "100%", padding: 8 }}
            value={customText}
            onChange={(e) => setCustomText(e.target.value)}
            placeholder="Paste Chinese or Vietnamese text here"
          />
          <div style={{ marginTop: 8 }}>
            <button
              style={{ marginLeft: 0 }}
              onClick={() => {
                if (!customText || customText.trim() === "") return;
                axios
                  .post("/api/samples/custom", {
                    text: customText,
                  })
                  .then((r) => setCustomSample(r.data))
                  .catch(() => alert("Failed to create custom sample"));
              }}
            >
              Use text
            </button>
          </div>
          {customSample && (
            <div style={{ marginTop: 12 }}>
              <div style={{ fontSize: 20 }}>{customSample.text}</div>
              {showPinyin && (
                <div style={{ color: "#666", marginTop: 6 }}>
                  {customSample.pinyin}
                </div>
              )}
              <TypingBox target={customSample.text} />
              <div style={{ marginTop: 8, color: "#666" }} />
            </div>
          )}
        </div>
        {samples.map((s) => (
          <div key={s.id} style={{ marginBottom: 18 }}>
            <div style={{ fontSize: 20 }}>{s.text}</div>
            {showPinyin && <PinyinLine text={s.text} />}
            <TypingBox target={s.text} />
          </div>
        ))}
      </div>
    </div>
  );
}

function PinyinLine({ text }) {
  const [pinyin, setPinyin] = useState("");
  useEffect(() => {
    axios
      .get("/api/pinyin", { params: { text } })
      .then((r) => setPinyin(r.data.pinyin));
  }, [text]);
  return <div style={{ color: "#666", marginTop: 6 }}>{pinyin}</div>;
}

function TypingBox({ target }) {
  const [value, setValue] = useState("");
  const [ok, setOk] = useState(null);
  const [expectedRaw, setExpectedRaw] = useState("");
  const [expectedNorm, setExpectedNorm] = useState("");
  const [isComposing, setIsComposing] = useState(false);
  const [hanDiff, setHanDiff] = useState({ target: [], input: [] });
  const normalize = (s) => {
    if (!s) return "";
    // normalize common user input variations:
    // - convert fullwidth latin chars to ASCII
    // - remove diacritics (tone marks)
    // - accept 'v' or 'u:' or 'ü' as 'u'
    // - remove digits (tone numbers), punctuation and spaces
    try {
      // fullwidth latin -> ascii
      s = s.replace(/[\uFF01-\uFF5E]/g, (ch) =>
        String.fromCharCode(ch.charCodeAt(0) - 0xfee0),
      );
      // decompose and strip diacritics
      if (s.normalize) s = s.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    } catch (e) {
      // ignore
    }
    // common pinyin variations: v -> u (many IMEs use v for ü), u: -> u, ü -> u
    s = s.replace(/v/g, "u").replace(/u:/g, "u").replace(/ü/g, "u");
    // lowercase and strip everything except a-z
    return s.toLowerCase().replace(/[^a-z]/g, "");
  };

  const normalizeChinese = (s) => {
    if (!s) return "";
    // NFC, remove surrounding whitespace, collapse spaces
    try {
      s = s.normalize("NFC");
    } catch (e) {}
    // map fullwidth latin/punct to ascii
    s = s.replace(/[\uFF01-\uFF5E]/g, (ch) =>
      String.fromCharCode(ch.charCodeAt(0) - 0xfee0),
    );
    // remove common punctuation/symbols and whitespace but keep CJK characters
    s = s.replace(
      /[\u2000-\u206F\u3000-\u303F\uFF00-\uFFEF\u0000-\u002F\u003A-\u0040\u005B-\u0060\u007B-\u007E\s]+/g,
      "",
    );
    return s.trim();
  };

  // Avoid using Unicode property escapes for broader compatibility.
  // Test common CJK ranges: Extension A, Unified, Compatibility.
  const containsCJK = (s) =>
    /[\u3400-\u4DBF\u4E00-\u9FFF\uF900-\uFAFF]/.test(s);

  const punctuationMap = {
    "，": ",",
    "。": ".",
    "！": "!",
    "？": "?",
    "：": ":",
    "；": ";",
    "（": "(",
    "）": ")",
    "、": ",",
    "“": '"',
    "”": '"',
    "‘": "'",
    "’": "'",
    "—": "-",
    "《": "<",
    "》": ">",
  };

  const mapPunctuation = (s) => {
    if (!s) return "";
    try {
      s = s.normalize("NFC");
    } catch (e) {}
    // fullwidth ASCII -> ASCII
    s = s.replace(/[\uFF01-\uFF5E]/g, (ch) =>
      String.fromCharCode(ch.charCodeAt(0) - 0xfee0),
    );
    // map common Chinese punctuation to ascii equivalents
    s = s
      .split("")
      .map((ch) => punctuationMap[ch] ?? ch)
      .join("");
    // collapse multiple spaces
    s = s.replace(/\s+/g, " ").trim();
    return s;
  };

  const normalizeChineseEquality = (s) => {
    // remove punctuation and whitespace for equality check
    const mapped = mapPunctuation(s);
    return mapped.replace(
      /[\u2000-\u206F\u3000-\u303F\uFF00-\uFFEF\u0000-\u002F\u003A-\u0040\u005B-\u0060\u007B-\u007E\s]+/g,
      "",
    );
  };

  const diffHanText = (targetText, inputText) => {
    const targetChars = mapPunctuation(targetText).split("");
    const inputChars = mapPunctuation(inputText).split("");
    const targetCells = [];
    const inputCells = [];

    let i = 0;
    let j = 0;
    while (i < targetChars.length || j < inputChars.length) {
      const targetCh = targetChars[i];
      const inputCh = inputChars[j];

      if (targetCh === undefined) {
        inputCells.push({ ch: inputCh, status: "extra" });
        j += 1;
        continue;
      }

      if (inputCh === undefined) {
        targetCells.push({ ch: targetCh, status: "missing" });
        i += 1;
        continue;
      }

      if (targetCh === inputCh) {
        targetCells.push({ ch: targetCh, status: "ok" });
        inputCells.push({ ch: inputCh, status: "ok" });
        i += 1;
        j += 1;
        continue;
      }

      const nextTarget = targetChars[i + 1];
      const nextInput = inputChars[j + 1];

      if (nextTarget === inputCh) {
        targetCells.push({ ch: targetCh, status: "missing" });
        i += 1;
        continue;
      }

      if (nextInput === targetCh) {
        inputCells.push({ ch: inputCh, status: "extra" });
        j += 1;
        continue;
      }

      targetCells.push({ ch: targetCh, status: "wrong" });
      inputCells.push({ ch: inputCh, status: "wrong" });
      i += 1;
      j += 1;
    }

    return { target: targetCells, input: inputCells };
  };

  const submit = () => {
    if (isComposing) {
      // don't validate while IME composition is active
      console.log("submit while composing - ignoring");
      return;
    }

    // If user entered Chinese characters, compare directly to target (normalize both)
    if (containsCJK(value)) {
      const userNorm = normalizeChineseEquality(value);
      const tgtNorm = normalizeChineseEquality(target);
      console.log("han-compare", { userNorm, tgtNorm });
      setExpectedRaw(target);
      setExpectedNorm(tgtNorm);
      // set ok for whole-string equality
      setOk(userNorm === tgtNorm);
      setHanDiff(diffHanText(target, value));
      return;
    }

    // otherwise treat input as pinyin
    setHanDiff({ target: [], input: [] });
    axios.get("/api/pinyin", { params: { text: target } }).then((r) => {
      const expected = normalize(r.data.pinyin);
      const got = normalize(value);
      setExpectedRaw(r.data.pinyin);
      setExpectedNorm(expected);
      console.log("pinyin-compare", {
        expected: r.data.pinyin,
        expectedNorm: expected,
        gotRaw: value,
        gotNorm: got,
      });
      setOk(expected === got);
    });
  };
  return (
    <div style={{ marginTop: 6 }}>
      <input
        value={value}
        onChange={(e) => setValue(e.target.value)}
        onCompositionStart={() => setIsComposing(true)}
        onCompositionEnd={(e) => {
          setIsComposing(false);
          setValue(e.target.value);
        }}
        placeholder="Type pinyin here..."
      />
      <button onClick={submit} style={{ marginLeft: 8 }}>
        Check
      </button>
      {ok !== null && <span style={{ marginLeft: 8 }}>{ok ? "✅" : "❌"}</span>}
      {expectedRaw && (
        <div style={{ marginTop: 6, color: "#666" }}>
          <div>Expected: {expectedRaw}</div>
          <div>Normalized expected: {expectedNorm}</div>
        </div>
      )}
      {hanDiff.target.length > 0 && (
        <div style={{ marginTop: 10 }}>
          <div style={{ color: "#666", marginBottom: 4 }}>Target</div>
          <div style={{ marginBottom: 8 }}>
            {hanDiff.target.map((d, i) => (
              <span
                key={`t-${i}`}
                style={{
                  padding: "2px 4px",
                  marginRight: 2,
                  background:
                    d.status === "ok"
                      ? "#c8f7c5"
                      : d.status === "missing"
                        ? "#ffd6d6"
                        : "#fff3bf",
                  borderRadius: 3,
                }}
              >
                {d.ch}
              </span>
            ))}
          </div>
          <div style={{ color: "#666", marginBottom: 4 }}>Your input</div>
          <div>
            {hanDiff.input.map((d, i) => (
              <span
                key={`u-${i}`}
                style={{
                  padding: "2px 4px",
                  marginRight: 2,
                  background:
                    d.status === "ok"
                      ? "#c8f7c5"
                      : d.status === "extra"
                        ? "#ffd6d6"
                        : "#fff3bf",
                  borderRadius: 3,
                }}
              >
                {d.ch}
              </span>
            ))}
          </div>
        </div>
      )}
      {isComposing && (
        <div style={{ color: "#c66", marginTop: 6 }}>
          IME composing... finish composition then press Check
        </div>
      )}
    </div>
  );
}
