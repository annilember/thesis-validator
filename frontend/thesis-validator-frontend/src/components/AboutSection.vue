<script setup lang="ts">
import { ref } from 'vue'

const faqs = [
  {
    question: 'Mida rakendus täpsemalt kontrollib?',
    answer: 'Rakendus kontrollib lehe veeriseid, pealkirjade kujundust, teksti joondust, dokumendi osade järjekorda ja muid struktuuri- ja vormistusnõudeid.'
  },
  {
    question: 'Millises formaadis fail üles laadida?',
    answer: 'Rakendus toetab hetkel ainult DOCX formaati. ODT, PDF, LaTeX ja muud formaadid ei ole toetatud.'
  },
  {
    question: 'Kas minu fail salvestatakse?',
    answer: 'Ei – üleslaaditud dokumente ei salvestata serverisse. Faili töödeldakse ainult valideerimise ajal ja kustutatakse seejärel.'
  }
]

const openIndex = ref<number | null>(null)

const toggle = (index: number) => {
  openIndex.value = openIndex.value === index ? null : index
}
</script>

<template>
  <section id="about" class="space-y-6">
    <div>
      <h2 class="text-xl font-semibold text-gray-900">Info</h2>
      <p class="mt-2 text-gray-600">
        Lõputöö validaator on prototüüpne veebirakendus, mis kontrollib automaatselt
        akadeemiliste lõputööde vastavust struktuuri- ja vormistusnõuetele.
      </p>
    </div>

    <div class="space-y-2">
      <div v-for="(faq, index) in faqs" :key="index" class="border border-gray-200 rounded-lg overflow-hidden">
        <button @click="toggle(index)"
          class="w-full flex items-center justify-between px-5 py-4 text-left hover:bg-gray-50 transition-colors">
          <span class="font-medium text-gray-900 text-sm">{{ faq.question }}</span>
          <span class="text-gray-400 ml-4">{{ openIndex === index ? '−' : '+' }}</span>
        </button>
        <div v-if="openIndex === index" class="px-5 pb-4 text-sm text-gray-600">
          {{ faq.answer }}
        </div>
      </div>
    </div>
  </section>
</template>
