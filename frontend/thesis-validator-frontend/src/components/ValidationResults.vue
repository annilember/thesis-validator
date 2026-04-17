<script setup lang="ts">
import type { IValidationResponse } from '@/types/IValidationResponse'
import { computed, ref, watch } from 'vue';

const props = defineProps<{
  result: IValidationResponse
}>()

type Filter = 'all' | 'passed' | 'failed' | 'skipped'
const activeFilter = ref<Filter>('all')

const passedCount = computed(() => props.result.issues.filter(i => i.passed).length)
const failedCount = computed(() => props.result.issues.filter(i => !i.passed && !i.skipped).length)
const skippedCount = computed(() => props.result.issues.filter(i => i.skipped).length)

const filteredIssues = computed(() => {
  switch (activeFilter.value) {
    case 'passed': return props.result.issues.filter(i => i.passed)
    case 'failed': return props.result.issues.filter(i => !i.passed && !i.skipped)
    case 'skipped': return props.result.issues.filter(i => i.skipped)
    default: return props.result.issues
  }
})

const expandedIssues = ref(new Set<string>())

watch(activeFilter, (newFilter) => {
  expandedIssues.value = new Set(
    filteredIssues.value
      .filter(i => i.details && (newFilter === 'failed' || newFilter === 'skipped'))
      .map(i => i.ruleId)
  )
})

const toggleDetails = (ruleId: string) => {
  if (expandedIssues.value.has(ruleId)) {
    expandedIssues.value.delete(ruleId)
  } else {
    expandedIssues.value.add(ruleId)
  }
}
</script>

<template>
  <div class="space-y-6">
    <h2 class="text-xl font-semibold text-gray-900">Valideerimistulemused</h2>

    <!-- Kokkuvõte + filter -->
    <div class="flex gap-4">
      <button @click="activeFilter = 'all'"
        class="flex-1 rounded-lg p-4 text-center border transition-all cursor-pointer"
        :class="activeFilter === 'all' ? 'bg-gray-100 border-gray-400' : 'bg-gray-50 border-gray-200 hover:border-gray-300'">
        <p class="text-2xl font-semibold text-gray-700">{{ result.issues.length }}</p>
        <p class="text-sm text-gray-600">Kõik</p>
      </button>
      <button @click="activeFilter = 'passed'"
        class="flex-1 rounded-lg p-4 text-center border transition-all cursor-pointer"
        :class="activeFilter === 'passed' ? 'bg-green-100 border-green-400' : 'bg-green-50 border-green-200 hover:border-green-300'">
        <p class="text-2xl font-semibold text-green-700">{{ passedCount }}</p>
        <p class="text-sm text-green-600">Läbitud</p>
      </button>
      <button @click="activeFilter = 'failed'"
        class="flex-1 rounded-lg p-4 text-center border transition-all cursor-pointer"
        :class="activeFilter === 'failed' ? 'bg-red-100 border-red-400' : 'bg-red-50 border-red-200 hover:border-red-300'">
        <p class="text-2xl font-semibold text-red-700">{{ failedCount }}</p>
        <p class="text-sm text-red-600">Ebaõnnestus</p>
      </button>
      <button @click="activeFilter = 'skipped'"
        class="flex-1 rounded-lg p-4 text-center border transition-all cursor-pointer"
        :class="activeFilter === 'skipped' ? 'bg-gray-200 border-gray-400' : 'bg-gray-50 border-gray-200 hover:border-gray-300'">
        <p class="text-2xl font-semibold text-gray-700">{{ skippedCount }}</p>
        <p class="text-sm text-gray-600">Vahele jäetud</p>
      </button>
    </div>

    <!-- Tulemuste loend -->
    <div v-for="issue in filteredIssues" :key="issue.ruleId" class="rounded-lg border" :class="{
      'bg-green-50 border-green-200': issue.passed,
      'bg-red-50 border-red-200': !issue.passed && !issue.skipped && issue.severity === 'Error',
      'bg-yellow-50 border-yellow-200': !issue.passed && !issue.skipped && issue.severity === 'Warning',
      'bg-gray-50 border-gray-200': issue.skipped
    }">
      <div class="flex items-center gap-3 p-4" :class="issue.details ? 'cursor-pointer' : ''"
        @click="issue.details && toggleDetails(issue.ruleId)">
        <span class="text-lg shrink-0">
          {{ issue.passed ? '✓' : issue.skipped ? '–' : '✗' }}
        </span>
        <div class="flex-1">
          <div class="flex items-center gap-2">
            <p class="text-sm font-medium text-gray-900">{{ issue.message }}</p>
            <span v-if="!issue.passed && !issue.skipped && issue.severity === 'Warning'"
              class="text-xs px-1.5 py-0.5 rounded bg-yellow-100 text-yellow-700 border border-yellow-200">
              soovituslik
            </span>
          </div>
        </div>
        <span v-if="issue.details" class="text-gray-900 text-sm shrink-0 cursor-pointer">
          {{ expandedIssues.has(issue.ruleId) ? '▲' : '▼' }}
        </span>
      </div>
      <div v-if="issue.details && expandedIssues.has(issue.ruleId)" class="pl-10 pb-4 text-sm italic text-red-600">
        {{ issue.details }}
      </div>
    </div>
  </div>
</template>
