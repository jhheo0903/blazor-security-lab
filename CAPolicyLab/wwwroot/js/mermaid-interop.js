/**
 * mermaid-interop.js
 * Blazor JSInterop 과 Mermaid.js 를 연결하는 브릿지.
 *
 * App.razor 에서 mermaid.initialize() 로 초기화된 후 이 파일이 로드된다.
 * MermaidDiagram.razor 의 OnAfterRenderAsync 에서 mermaidInterop.render() 를 호출한다.
 */
window.mermaidInterop = {
    /**
     * Mermaid 다이어그램을 렌더링하여 지정 컨테이너에 삽입한다.
     * @param {string} containerId - 다이어그램을 삽입할 DOM 요소 id
     * @param {string} definition  - Mermaid 다이어그램 정의 문자열
     */
    render: async function (containerId, definition) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.warn(`[mermaidInterop] 컨테이너를 찾을 수 없음: #${containerId}`);
            return;
        }

        try {
            // mermaid.render() 는 고유한 SVG id 가 필요함
            // containerId 를 기반으로 생성하되, Mermaid 가 허용하는 형식으로 변환
            const svgId = 'svg-' + containerId.replace(/[^a-zA-Z0-9]/g, '-');

            const { svg } = await mermaid.render(svgId, definition);

            // 로딩 스피너를 제거하고 SVG 삽입
            container.innerHTML = svg;

            // SVG 가 컨테이너 너비에 맞게 표시되도록 스타일 조정
            const svgEl = container.querySelector('svg');
            if (svgEl) {
                svgEl.style.maxWidth = '100%';
                svgEl.style.height = 'auto';
            }
        } catch (err) {
            // Mermaid 파싱 오류 시 사람이 읽을 수 있는 오류 메시지 표시
            console.error('[mermaidInterop] 렌더링 오류:', err);
            container.innerHTML = `
                <div class="alert alert-warning small">
                    <strong>다이어그램 렌더링 오류</strong><br>
                    ${err.message || '알 수 없는 오류'}
                </div>`;
        }
    }
};
