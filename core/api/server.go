package api

import (
	"encoding/json"
	"fmt"
	"log"
	"net"
	"net/http"
	"sync"
	"time"

	"miair-core/miservice"
	"miair-core/playback"
	"miair-core/source"
)

type Server struct {
	Port        int
	Version     string
	Account     *miservice.Account
	Coordinator *playback.Coordinator
	Manager     *source.Manager

	mu             sync.RWMutex
	targetDID      string
	name           string
	airplayEnabled bool
	dlnaEnabled    bool
	bufferMs       int
	sourcePolicy   string
	preferredProto string
	httpLn         net.Listener
	httpServer     *http.Server
}

type ConfigInfo struct {
	Name              string `json:"name"`
	TargetDID         string `json:"target_did"`
	AirPlayEnabled    bool   `json:"airplay_enabled"`
	DLNAEnabled       bool   `json:"dlna_enabled"`
	BufferMs          int    `json:"buffer_ms"`
	SourcePolicy      string `json:"source_policy"`
	PreferredProtocol string `json:"preferred_protocol"`
	Version           string `json:"version"`
}

type StatusResponse struct {
	Running   bool                   `json:"running"`
	Version   string                 `json:"version"`
	UpdatedAt time.Time              `json:"updated_at"`
	Source    source.Snapshot        `json:"source"`
	Token     *miservice.TokenStatus `json:"token,omitempty"`
	Config    ConfigInfo             `json:"config"`
}

func NewServer(port int, version string, account *miservice.Account, coordinator *playback.Coordinator, manager *source.Manager) *Server {
	return &Server{
		Port:        port,
		Version:     version,
		Account:     account,
		Coordinator: coordinator,
		Manager:     manager,
	}
}

func (s *Server) SetConfig(name, targetDID string, airplay, dlna bool, bufferMs int, policy, preferred string) {
	s.mu.Lock()
	defer s.mu.Unlock()
	s.name = name
	s.targetDID = targetDID
	s.airplayEnabled = airplay
	s.dlnaEnabled = dlna
	s.bufferMs = bufferMs
	s.sourcePolicy = policy
	s.preferredProto = preferred
}

func (s *Server) Start() error {
	mux := http.NewServeMux()

	mux.HandleFunc("/api/status", s.handleStatus)
	mux.HandleFunc("/api/qr", s.handleQR)
	mux.HandleFunc("/api/qr/poll", s.handleQRPoll)
	mux.HandleFunc("/api/devices", s.handleDevices)
	mux.HandleFunc("/api/speaker/bind", s.handleSpeakerBind)
	mux.HandleFunc("/api/speaker/pause", s.handleSpeakerPause)
	mux.HandleFunc("/api/speaker/volume", s.handleSpeakerVolume)
	mux.HandleFunc("/api/account/logout", s.handleAccountLogout)

	ln, err := net.Listen("tcp", fmt.Sprintf("127.0.0.1:%d", s.Port))
	if err != nil {
		return err
	}
	s.httpLn = ln
	s.httpServer = &http.Server{Handler: corsMiddleware(mux), ReadHeaderTimeout: 5 * time.Second}

	log.Printf("[API] REST control server listening on http://127.0.0.1:%d", s.Port)
	go func() {
		if err := s.httpServer.Serve(ln); err != nil && err != http.ErrServerClosed {
			log.Printf("[API] Server error: %v", err)
		}
	}()
	return nil
}

func (s *Server) Close() {
	if s.httpServer != nil {
		_ = s.httpServer.Close()
	}
}

func corsMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		w.Header().Set("Access-Control-Allow-Methods", "GET, POST, OPTIONS")
		w.Header().Set("Access-Control-Allow-Headers", "Content-Type")
		if r.Method == http.MethodOptions {
			w.WriteHeader(http.StatusOK)
			return
		}
		next.ServeHTTP(w, r)
	})
}

func (s *Server) handleStatus(w http.ResponseWriter, r *http.Request) {
	s.mu.RLock()
	cfg := ConfigInfo{
		Name:              s.name,
		TargetDID:         s.targetDID,
		AirPlayEnabled:    s.airplayEnabled,
		DLNAEnabled:       s.dlnaEnabled,
		BufferMs:          s.bufferMs,
		SourcePolicy:      s.sourcePolicy,
		PreferredProtocol: s.preferredProto,
		Version:           s.Version,
	}
	s.mu.RUnlock()

	var tokenStatus *miservice.TokenStatus
	if s.Account != nil {
		ts := s.Account.GetTokenStatus()
		tokenStatus = &ts
	}

	var snap source.Snapshot
	if s.Manager != nil {
		snap = s.Manager.Snapshot()
	}

	res := StatusResponse{
		Running:   true,
		Version:   s.Version,
		UpdatedAt: time.Now(),
		Source:    snap,
		Token:     tokenStatus,
		Config:    cfg,
	}

	writeJSON(w, http.StatusOK, res)
}

func (s *Server) handleQR(w http.ResponseWriter, r *http.Request) {
	if s.Account == nil {
		http.Error(w, "Account service unavailable", http.StatusServiceUnavailable)
		return
	}
	info, err := s.Account.GetQRLoginInfo()
	if err != nil {
		writeJSON(w, http.StatusInternalServerError, map[string]string{"error": err.Error()})
		return
	}
	writeJSON(w, http.StatusOK, map[string]interface{}{
		"qr":      info.QR,
		"lp":      info.LP,
		"timeout": info.Timeout,
	})
}

func (s *Server) handleQRPoll(w http.ResponseWriter, r *http.Request) {
	if s.Account == nil {
		http.Error(w, "Account service unavailable", http.StatusServiceUnavailable)
		return
	}
	lpURL := r.URL.Query().Get("lp")
	if lpURL == "" {
		http.Error(w, "Missing lp parameter", http.StatusBadRequest)
		return
	}
	status, data, err := s.Account.PollQRLogin(lpURL)
	if err != nil {
		writeJSON(w, http.StatusInternalServerError, map[string]string{"error": err.Error()})
		return
	}
	res := map[string]interface{}{
		"status": status,
	}
	if data != nil {
		res["userId"] = data.UserID
	}
	writeJSON(w, http.StatusOK, res)
}

func (s *Server) handleDevices(w http.ResponseWriter, r *http.Request) {
	if s.Account == nil {
		http.Error(w, "Account service unavailable", http.StatusServiceUnavailable)
		return
	}
	devs, err := s.Account.DeviceList(0)
	if err != nil {
		writeJSON(w, http.StatusInternalServerError, map[string]string{"error": err.Error()})
		return
	}
	writeJSON(w, http.StatusOK, map[string]interface{}{
		"devices": devs,
	})
}

func (s *Server) handleSpeakerBind(w http.ResponseWriter, r *http.Request) {
	var body struct {
		DID string `json:"did"`
	}
	if err := json.NewDecoder(r.Body).Decode(&body); err != nil {
		http.Error(w, "Invalid JSON", http.StatusBadRequest)
		return
	}
	s.mu.Lock()
	s.targetDID = body.DID
	s.mu.Unlock()
	writeJSON(w, http.StatusOK, map[string]bool{"success": true})
}

func (s *Server) handleSpeakerPause(w http.ResponseWriter, r *http.Request) {
	var body struct {
		DID string `json:"did"`
	}
	_ = json.NewDecoder(r.Body).Decode(&body)
	target := body.DID
	if target == "" {
		s.mu.RLock()
		target = s.targetDID
		s.mu.RUnlock()
	}
	if target == "" {
		http.Error(w, "No device specified", http.StatusBadRequest)
		return
	}
	if err := s.Account.PlayerPause(target); err != nil {
		writeJSON(w, http.StatusInternalServerError, map[string]string{"error": err.Error()})
		return
	}
	writeJSON(w, http.StatusOK, map[string]bool{"success": true})
}

func (s *Server) handleSpeakerVolume(w http.ResponseWriter, r *http.Request) {
	var body struct {
		DID    string `json:"did"`
		Volume int    `json:"volume"`
	}
	if err := json.NewDecoder(r.Body).Decode(&body); err != nil {
		http.Error(w, "Invalid JSON", http.StatusBadRequest)
		return
	}
	target := body.DID
	if target == "" {
		s.mu.RLock()
		target = s.targetDID
		s.mu.RUnlock()
	}
	if target == "" {
		http.Error(w, "No device specified", http.StatusBadRequest)
		return
	}
	if err := s.Account.PlayerSetVolume(target, body.Volume); err != nil {
		writeJSON(w, http.StatusInternalServerError, map[string]string{"error": err.Error()})
		return
	}
	writeJSON(w, http.StatusOK, map[string]bool{"success": true})
}

func (s *Server) handleAccountLogout(w http.ResponseWriter, r *http.Request) {
	if s.Account == nil {
		http.Error(w, "Account unavailable", http.StatusServiceUnavailable)
		return
	}
	s.Account.Data.UserID = ""
	s.Account.Data.PassToken = ""
	s.Account.Data.Tokens = make(map[string]miservice.TokenData)
	_ = s.Account.SaveStore()
	writeJSON(w, http.StatusOK, map[string]bool{"success": true})
}

func writeJSON(w http.ResponseWriter, status int, data interface{}) {
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(data)
}
