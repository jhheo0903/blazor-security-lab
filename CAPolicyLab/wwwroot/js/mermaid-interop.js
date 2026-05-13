/**
 * mermaid-interop.js
 * Mermaid.js 렌더링 + SVG 팬/줌 기능을 Blazor에서 제어할 수 있도록 연결한다.
 *
 * 상태는 containerId별로 _states에 보관한다.
 * 줌은 마우스 휠, 터치 핀치, +/- 버튼 세 가지로 제어한다.
 * 패닝은 드래그와 터치 슬라이드로 제어한다.
 */
window.mermaidInterop = {

    _states: {},

    // ── 렌더링 ──────────────────────────────────────────────────────────────

    render: async function (containerId, definition) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.warn('[mermaidInterop] 컨테이너 없음:', containerId);
            return;
        }

        // 이전 이벤트 리스너 정리
        this._cleanup(containerId);

        try {
            const svgId = 'svg-' + containerId.replace(/[^a-zA-Z0-9]/g, '-');
            const { svg } = await mermaid.render(svgId, definition);

            // 뷰포트 레이어: transform을 이 div에 적용한다.
            const viewport = document.createElement('div');
            viewport.className = 'mermaid-viewport';
            viewport.style.cssText =
                'display:inline-block; transform-origin:0 0; will-change:transform; line-height:0;';
            viewport.innerHTML = svg;

            container.innerHTML = '';
            container.appendChild(viewport);

            const svgEl = viewport.querySelector('svg');
            if (svgEl) {
                svgEl.style.display = 'block';
                svgEl.style.maxWidth = 'none';
            }

            this._initPanZoom(containerId);

            // 레이아웃 완료 후 화면에 맞게 축소
            requestAnimationFrame(() => this.fitToScreen(containerId));

        } catch (err) {
            console.error('[mermaidInterop] 렌더링 오류:', err);
            container.innerHTML = `
                <div class="alert alert-warning small m-3">
                    <i class="bi bi-exclamation-triangle me-2"></i>
                    <strong>다이어그램 렌더링 오류</strong><br>
                    ${err.message ?? '알 수 없는 오류'}
                </div>`;
        }
    },

    // ── 팬/줌 초기화 ────────────────────────────────────────────────────────

    _initPanZoom: function (containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const state = {
            scale: 1, panX: 0, panY: 0,
            dragging: false, startX: 0, startY: 0, basePanX: 0, basePanY: 0,
            lastTouchDist: 0
        };
        this._states[containerId] = state;

        const self = this;

        // 마우스 휠 줌 (마우스 위치 기준)
        const onWheel = (e) => {
            e.preventDefault();
            const factor = e.deltaY < 0 ? 1.12 : 1 / 1.12;
            const rect = container.getBoundingClientRect();
            const mx = e.clientX - rect.left;
            const my = e.clientY - rect.top;
            self._zoomAt(containerId, factor, mx, my);
        };

        // 드래그 패닝
        const onMousedown = (e) => {
            if (e.button !== 0) return;
            e.preventDefault();
            state.dragging = true;
            state.startX = e.clientX;
            state.startY = e.clientY;
            state.basePanX = state.panX;
            state.basePanY = state.panY;
            container.style.cursor = 'grabbing';
        };
        const onMousemove = (e) => {
            if (!state.dragging) return;
            state.panX = state.basePanX + (e.clientX - state.startX);
            state.panY = state.basePanY + (e.clientY - state.startY);
            self._applyTransform(containerId);
        };
        const onMouseup = () => {
            if (state.dragging) {
                state.dragging = false;
                container.style.cursor = 'grab';
            }
        };

        // 터치 패닝 + 핀치 줌
        const onTouchstart = (e) => {
            e.preventDefault();
            if (e.touches.length === 1) {
                state.dragging = true;
                state.startX = e.touches[0].clientX;
                state.startY = e.touches[0].clientY;
                state.basePanX = state.panX;
                state.basePanY = state.panY;
            } else if (e.touches.length === 2) {
                state.dragging = false;
                state.lastTouchDist = Math.hypot(
                    e.touches[0].clientX - e.touches[1].clientX,
                    e.touches[0].clientY - e.touches[1].clientY
                );
            }
        };
        const onTouchmove = (e) => {
            e.preventDefault();
            if (e.touches.length === 1 && state.dragging) {
                state.panX = state.basePanX + (e.touches[0].clientX - state.startX);
                state.panY = state.basePanY + (e.touches[0].clientY - state.startY);
                self._applyTransform(containerId);
            } else if (e.touches.length === 2) {
                const dist = Math.hypot(
                    e.touches[0].clientX - e.touches[1].clientX,
                    e.touches[0].clientY - e.touches[1].clientY
                );
                if (state.lastTouchDist > 0) {
                    const cx = (e.touches[0].clientX + e.touches[1].clientX) / 2
                        - container.getBoundingClientRect().left;
                    const cy = (e.touches[0].clientY + e.touches[1].clientY) / 2
                        - container.getBoundingClientRect().top;
                    self._zoomAt(containerId, dist / state.lastTouchDist, cx, cy);
                }
                state.lastTouchDist = dist;
            }
        };
        const onTouchend = () => { state.dragging = false; };

        container.addEventListener('wheel',      onWheel,      { passive: false });
        container.addEventListener('mousedown',  onMousedown);
        document.addEventListener('mousemove',   onMousemove);
        document.addEventListener('mouseup',     onMouseup);
        container.addEventListener('touchstart', onTouchstart, { passive: false });
        container.addEventListener('touchmove',  onTouchmove,  { passive: false });
        container.addEventListener('touchend',   onTouchend);

        // 정리용 핸들러 보관
        state._handlers = { onWheel, onMousedown, onMousemove, onMouseup,
                            onTouchstart, onTouchmove, onTouchend, container };
        container.style.cursor = 'grab';
    },

    // ── 내부 헬퍼 ───────────────────────────────────────────────────────────

    _zoomAt: function (containerId, factor, cx, cy) {
        const state = this._states[containerId];
        if (!state) return;
        const newScale = Math.max(0.05, Math.min(8, state.scale * factor));
        state.panX = cx - (cx - state.panX) * (newScale / state.scale);
        state.panY = cy - (cy - state.panY) * (newScale / state.scale);
        state.scale = newScale;
        this._applyTransform(containerId);
    },

    _applyTransform: function (containerId) {
        const state = this._states[containerId];
        const container = document.getElementById(containerId);
        if (!state || !container) return;
        const viewport = container.querySelector('.mermaid-viewport');
        if (viewport) {
            viewport.style.transform =
                `translate(${state.panX}px,${state.panY}px) scale(${state.scale})`;
        }
        // 줌 퍼센트 표시 업데이트
        const wrapper = container.closest('[data-mermaid-id]');
        const display = wrapper?.querySelector('.mermaid-zoom-pct');
        if (display) display.textContent = Math.round(state.scale * 100) + '%';
    },

    _cleanup: function (containerId) {
        const state = this._states[containerId];
        if (!state?._handlers) return;
        const h = state._handlers;
        h.container.removeEventListener('wheel',      h.onWheel);
        h.container.removeEventListener('mousedown',  h.onMousedown);
        document.removeEventListener('mousemove',     h.onMousemove);
        document.removeEventListener('mouseup',       h.onMouseup);
        h.container.removeEventListener('touchstart', h.onTouchstart);
        h.container.removeEventListener('touchmove',  h.onTouchmove);
        h.container.removeEventListener('touchend',   h.onTouchend);
        delete this._states[containerId];
    },

    // ── 버튼에서 호출되는 공개 API ───────────────────────────────────────────

    zoomIn: function (containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;
        const cx = container.clientWidth  / 2;
        const cy = container.clientHeight / 2;
        this._zoomAt(containerId, 1.25, cx, cy);
    },

    zoomOut: function (containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;
        const cx = container.clientWidth  / 2;
        const cy = container.clientHeight / 2;
        this._zoomAt(containerId, 1 / 1.25, cx, cy);
    },

    resetZoom: function (containerId) {
        const state = this._states[containerId];
        if (!state) return;
        state.scale = 1; state.panX = 0; state.panY = 0;
        this._applyTransform(containerId);
    },

    fitToScreen: function (containerId) {
        const container = document.getElementById(containerId);
        const state = this._states[containerId];
        if (!container || !state) return;

        const viewport = container.querySelector('.mermaid-viewport');
        const svgEl = viewport?.querySelector('svg');
        if (!svgEl) return;

        // 일시적으로 scale:1로 되돌려 자연 크기를 읽는다
        const prevTransform = viewport.style.transform;
        viewport.style.transform = 'translate(0,0) scale(1)';

        const svgW = svgEl.clientWidth  || svgEl.getBoundingClientRect().width;
        const svgH = svgEl.clientHeight || svgEl.getBoundingClientRect().height;

        viewport.style.transform = prevTransform;

        if (svgW === 0 || svgH === 0) return;

        const cW = container.clientWidth;
        const cH = container.clientHeight;
        const newScale = Math.min(cW / svgW, cH / svgH, 1) * 0.92;

        state.scale = Math.max(0.05, newScale);
        state.panX  = (cW - svgW * state.scale) / 2;
        state.panY  = (cH - svgH * state.scale) / 2;
        this._applyTransform(containerId);
    }
};
