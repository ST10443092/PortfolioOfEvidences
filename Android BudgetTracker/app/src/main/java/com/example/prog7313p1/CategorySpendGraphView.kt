package com.example.prog7313p1

import android.content.Context
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.Paint
import android.util.AttributeSet
import android.view.View
import kotlin.math.max

class CategorySpendGraphView @JvmOverloads constructor(
    context: Context,
    attrs: AttributeSet? = null
) : View(context, attrs) {

    data class CategorySpendData(
        val categoryName: String,
        val spent: Double,
        val minGoal: Double,
        val maxGoal: Double
    )

    private val axisPaint = Paint(Paint.ANTI_ALIAS_FLAG).apply {//(Android Developers, 2026b)
        color = Color.rgb(90, 90, 90)
        strokeWidth = 2f
    }
    private val barPaint = Paint(Paint.ANTI_ALIAS_FLAG).apply {//(Android Developers, 2026b)
        color = Color.rgb(63, 142, 252)
    }
    private val minPaint = Paint(Paint.ANTI_ALIAS_FLAG).apply {//(Android Developers, 2026b)
        color = Color.rgb(255, 152, 0)
        strokeWidth = 4f
    }
    private val maxPaint = Paint(Paint.ANTI_ALIAS_FLAG).apply {//(Android Developers, 2026b)
        color = Color.rgb(220, 53, 69)
        strokeWidth = 4f
    }
    private val textPaint = Paint(Paint.ANTI_ALIAS_FLAG).apply {//(Android Developers, 2026b)
        color = Color.rgb(43, 43, 43)
        textSize = 24f
        textAlign = Paint.Align.CENTER
    }
    private val emptyPaint = Paint(Paint.ANTI_ALIAS_FLAG).apply {//(Android Developers, 2026b)
        color = Color.rgb(107, 107, 107)
        textSize = 28f
        textAlign = Paint.Align.CENTER
    }

    private var data: List<CategorySpendData> = emptyList()//(Android Developers, 2026b)

    fun setData(newData: List<CategorySpendData>) {
        data = newData
        invalidate()
    }

    override fun onDraw(canvas: Canvas) {
        super.onDraw(canvas)

        if (data.isEmpty()) {
            canvas.drawText("No spending data for this period", width / 2f, height / 2f, emptyPaint)
            return
        }

        val left = 48f
        val top = 16f
        val right = width - 16f
        val bottom = height - 42f
        val graphHeight = bottom - top
        val graphWidth = right - left
        val maxValue = data.maxOf {
            max(it.spent, max(it.minGoal, it.maxGoal))
        }.coerceAtLeast(1.0)

        canvas.drawLine(left, top, left, bottom, axisPaint)//(Android Developers, 2026a)
        canvas.drawLine(left, bottom, right, bottom, axisPaint)//(Android Developers, 2026a)

        val slotWidth = graphWidth / data.size
        val barWidth = slotWidth * 0.48f

        data.forEachIndexed { index, item ->
            val centerX = left + slotWidth * index + slotWidth / 2f
            val barTop = valueToY(item.spent, maxValue, top, graphHeight)
            canvas.drawRect(centerX - barWidth / 2f, barTop, centerX + barWidth / 2f, bottom, barPaint)

            drawGoalMarker(canvas, item.minGoal, maxValue, centerX, barWidth, top, graphHeight)//(Android Developers, 2026a)
            drawGoalMarker(canvas, item.maxGoal, maxValue, centerX, barWidth, top, graphHeight, isMax = true)//(Android Developers, 2026a)

            canvas.drawText(shortLabel(item.categoryName), centerX, height - 12f, textPaint)
        }
    }

    private fun valueToY(value: Double, maxValue: Double, top: Float, graphHeight: Float): Float {
        return top + graphHeight - ((value / maxValue) * graphHeight).toFloat()
    }

    private fun drawGoalMarker(
        canvas: Canvas,
        value: Double,
        maxValue: Double,
        centerX: Float,
        barWidth: Float,
        top: Float,
        graphHeight: Float,
        isMax: Boolean = false
    ) {
        if (value <= 0.0) return

        val y = valueToY(value, maxValue, top, graphHeight)
        val paint = if (isMax) maxPaint else minPaint
        canvas.drawLine(centerX - barWidth / 2f, y, centerX + barWidth / 2f, y, paint)
    }

    private fun shortLabel(label: String): String {
        return if (label.length <= 6) label else label.take(6)
    }
}
/*
    Harvard References
    Android Developers, 2024a. Create a custom drawing. Available at: https://developer.android.com/develop/ui/views/layout/custom-views/custom-drawing - Create a Custom Drawing [Accessed 31 May 2026].
    Android Developers, 2026b. Use common Kotlin patterns with Android. Available at: https://developer.android.com/kotlin/common-patterns - Common Kotlin Patterns [Accessed 31 May 2026].
 */