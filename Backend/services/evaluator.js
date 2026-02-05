class SessionEvaluator {
  constructor() {
    this.startedAt = null;
    this.endedAt = null;

    this.currentStep = "START";
    this.currentTool = null;

    this.score = 100;
    this.mistakes = [];
    this.mistakesSummary = {};
    this.metricsSummary = { maxDepthMm: 0, avgAngleDeg: 0 };

    this._angleSum = 0;
    this._angleCount = 0;
  }

  start(tsIso) { this.startedAt = new Date(tsIso); }
  end(tsIso) { this.endedAt = new Date(tsIso); }

  setStep(step) { this.currentStep = step; }
  setTool(toolId) { this.currentTool = toolId; }

  recordMistake(errorType, penalty, evidence) {
    this.score = Math.max(0, this.score - penalty);

    this.mistakes.push({
      step: this.currentStep,
      errorType,
      penaltyApplied: penalty,
      evidence
    });

    this.mistakesSummary[errorType] = (this.mistakesSummary[errorType] || 0) + 1;

    return { applied: penalty };
  }

  evaluateToolAllowed() {
    if (this.currentStep === "DRILLING") {
      const allowed = ["HIGH_SPEED_BUR", "ROUND_BUR"];
      if (!allowed.includes(this.currentTool)) {
        return this.recordMistake("WRONG_TOOL", 8, { tool: this.currentTool, allowed });
      }
    }
    return null;
  }

  drillSample(depthMm, angleDeg) {
    this.metricsSummary.maxDepthMm = Math.max(this.metricsSummary.maxDepthMm, depthMm);
    this._angleSum += angleDeg;
    this._angleCount += 1;
    this.metricsSummary.avgAngleDeg = this._angleSum / this._angleCount;

    if (this.currentStep !== "DRILLING") return null;

    if (depthMm > 2.0) {
      return this.recordMistake("TOO_DEEP", 20, { depthMm, max: 2.0 });
    }
    if (angleDeg > 20) {
      return this.recordMistake("ANGLE_TOO_STEEP", 10, { angleDeg, max: 20 });
    }
    return this.evaluateToolAllowed();
  }

  finalize() {
    const durationSec = this.startedAt && this.endedAt
      ? Math.floor((this.endedAt - this.startedAt) / 1000)
      : 0;

    return {
      startedAt: this.startedAt,
      endedAt: this.endedAt,
      durationSec,
      finalScore: this.score,
      mistakesSummary: this.mistakesSummary,
      metricsSummary: this.metricsSummary
    };
  }
}

module.exports = { SessionEvaluator };

