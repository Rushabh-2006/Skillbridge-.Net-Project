// SkillBridge Chart.js Utilities
const SkillBridgeCharts = {
  renderMissingSkillsChart(canvasId, labels, data) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    new Chart(ctx, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [{
          label: 'Students Lacking Skill',
          data: data,
          backgroundColor: '#3B82F6',
          borderRadius: 6,
          borderSkipped: false
        }]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        plugins: {
          legend: { display: false }
        },
        scales: {
          x: {
            grid: { color: '#334155' },
            ticks: { color: '#94A3B8' }
          },
          y: {
            grid: { display: false },
            ticks: { color: '#F8FAFC', font: { weight: '600' } }
          }
        }
      }
    });
  },

  renderSkillGapChart(canvasId, heldCount, missingCount) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: ['Skills Held', 'Skills Missing'],
        datasets: [{
          data: [heldCount, missingCount],
          backgroundColor: ['#10B981', '#EF4444'],
          borderWidth: 0
        }]
      },
      options: {
        responsive: true,
        plugins: {
          legend: { labels: { color: '#F8FAFC' } }
        },
        cutout: '75%'
      }
    });
  }
};
